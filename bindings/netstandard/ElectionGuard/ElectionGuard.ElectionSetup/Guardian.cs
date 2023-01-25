using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Bson.IO;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElectionGuard.ElectionSetup;


public record GuardianPrivateRecord(
    string GuardianId,
    ElectionKeyPair ElectionKeys,
    Dictionary<string, ElectionPartialKeyBackup>? BackupsToShare,
    Dictionary<string, ElectionPublicKey>? GuardianElectionPublicKeys,
    Dictionary<string, ElectionPartialKeyBackup>? GuardianElectionPartialKeyBackups,
    Dictionary<string, ElectionPartialKeyVerification>? GuardianElectionPartialKeyVerifications
    );

public record GuardianRecord(
    string GuardianId,
    ulong SequenceOrder,
    ElementModP ElectionPublicKey,
    List<ElementModP> ElectionCommitments,
    List<SchnorrProof> ElectionProofs
    );

/// <summary>
/// Guardian of election responsible for safeguarding information and decrypting results.
///
/// The first half of the guardian involves the key exchange known as the key ceremony.
/// The second half relates to the decryption process.
/// </summary>
public class Guardian : DisposableBase
{
    private const string GUARDIAN_PREFIX = "guardian_";
    private const string PRIVATE_KEY_FOLDER = "gui_private_keys";
    private const string GUARDIAN_EXT = ".json";

    private readonly ElectionKeyPair _electionKeys;
    private readonly Dictionary<string, ElectionPublicKey>? _otherGuardianPublicKeys;
    private readonly Dictionary<string, ElectionPartialKeyBackup>? _otherGuardianPartialKeyBackups;
    private Dictionary<string, ElectionPartialKeyVerification>? _otherGuardianPartialKeyVerification = new();

    public string GuardianId { get; set; }
    public ulong SequenceOrder { get; set; }
    public CeremonyDetails CeremonyDetails { get; set; }
    public Dictionary<string, ElectionPartialKeyBackup> BackupsToShare { get; set; } = new();

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    /// <param name="otherGuardianPublicKeys">The public keys the guardian generated during a key ceremony</param>
    /// <param name="otherGuardianPartialKeyBackups">The partial key backups the guardian generated during a key ceremony</param>
    /// <param name="partialKeyBackup"></param>
    /// <param name="guardianElectionPartialKeyVerifications"></param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails,
        Dictionary<string, ElectionPublicKey>? otherGuardianPublicKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherGuardianPartialKeyBackups = null,
        Dictionary<string, ElectionPartialKeyBackup>? partialKeyBackup = null,
        Dictionary<string, ElectionPartialKeyVerification>? guardianElectionPartialKeyVerifications = null
        )
    {
        _electionKeys = keyPair;
        GuardianId = keyPair.OwnerId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        _otherGuardianPublicKeys = otherGuardianPublicKeys;
        _otherGuardianPartialKeyBackups = otherGuardianPartialKeyBackups;

        if (partialKeyBackup != null)
        {
            BackupsToShare = partialKeyBackup;
        }

        if (guardianElectionPartialKeyVerifications != null)
        {
            _otherGuardianPartialKeyVerification = guardianElectionPartialKeyVerifications;
        }

        SaveGuardianKey(keyPair.Share());
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        _electionKeys.Dispose();
        _otherGuardianPublicKeys?.Dispose();
        _otherGuardianPartialKeyBackups?.Dispose();
        BackupsToShare.Dispose();
    }

    //private void GenerateBackupKeys()
    //{
    //    Parallel.For(0, CeremonyDetails.numberOfGuardians, (i) =>
    //    {
    //        if (i == _electionKeys.SequenceOrder)
    //        {
    //            // Don't generate a backup for your own key.
    //            return;
    //        }

    //        BackupsToShare.Add(i.ToString(), GenerateElectionPartialKeyBackup((ulong)i));
    //    });
    //}

    /// <summary>
    /// Share a backup key 
    /// </summary>
    /// <param name="sequenceOrder"></param>
    /// <returns></returns>
    public ElectionPartialKeyBackup Share(string sequenceOrder) => BackupsToShare[sequenceOrder];


    public static Guardian FromPublicKey(
        int numberOfGuardians,
        int quorum,
        string keyCeremonyId,
        ElectionPublicKey publicKey)
    {
        var elgamalKeyPair = ElGamalKeyPair.FromPair(Constants.ZERO_MOD_Q, publicKey.Key);
        var keyPair = new ElectionKeyPair(
            publicKey.OwnerId,
            publicKey.SequenceOrder,
            elgamalKeyPair,
            new ElectionPolynomial(new()));
        CeremonyDetails ceremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);
        return new(keyPair, ceremonyDetails);
    }

    public static Guardian FromNonce(
        string guardianId,
        ulong sequenceOrder,
        int numberOfGuardians,
        int quorum,
        string keyCeremonyId,
        ElementModQ? nonce = null)
    {
        var keyPair = ElectionKeyPair.GenerateElectionKeyPair(guardianId, sequenceOrder, quorum, nonce);
        CeremonyDetails ceremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);
        return new(keyPair, ceremonyDetails);
    }

    public static Guardian FromPrivateRecord(
        GuardianPrivateRecord privateGuardianRecord,
        string keyCeremonyId,
        int numberOfGuardians,
        int quorum)
    {
        return new(
            privateGuardianRecord.ElectionKeys,
            new(keyCeremonyId, numberOfGuardians, quorum),
            privateGuardianRecord.GuardianElectionPublicKeys,
            privateGuardianRecord.GuardianElectionPartialKeyBackups,
            privateGuardianRecord.BackupsToShare,
            privateGuardianRecord.GuardianElectionPartialKeyVerifications);
    }


    //    private ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(ulong sequenceOrder)
    private ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(string senderGuardianId, ElectionPolynomial senderGuardianPolynomial, ElectionPublicKey receiverGuardianPublicKey)
    {
        var coordinate = ComputePolynomialCoordinate(receiverGuardianPublicKey.SequenceOrder, senderGuardianPolynomial);
        using var nonce = BigMath.RandQ();
        var seed = GetBackupSeed(
                receiverGuardianPublicKey.OwnerId,
                receiverGuardianPublicKey.SequenceOrder);

        var data = coordinate.ToBytes();
        var encryptedCoordinate = HashedElgamal.Encrypt(data, (ulong)data.Length, nonce, receiverGuardianPublicKey.Key, seed);

        return new()
        {
            OwnerId = senderGuardianId,
            DesignedId = receiverGuardianPublicKey.OwnerId,
            DesignatedSequenceOrder = receiverGuardianPublicKey.SequenceOrder,
            EncryptedCoordinate = encryptedCoordinate
        };
    }

    private ElementModQ GetBackupSeed(string ownerId, ulong? sequenceOrder)
    {
        return BigMath.HashElems(ownerId, sequenceOrder ?? 0);
    }

    private ElementModQ ComputePolynomialCoordinate(ulong sequenceOrder, ElectionPolynomial? polynomial = null)
    {
        using var sequenceOrderModQ = new ElementModQ(sequenceOrder);
        var coordinate = Constants.ZERO_MOD_Q; // start at 0 mod q.

        var coefficients = polynomial != null ? polynomial.Coefficients : _electionKeys.Polynomial.Coefficients;

        foreach (var (coefficient, index) in coefficients.WithIndex())
        {
            coordinate = GetCoordinate(sequenceOrderModQ, coordinate, coefficient, index);
        }

        return coordinate;
    }

    private static ElementModQ GetCoordinate(
        ElementModQ initialState,
        ElementModQ baseElement,
        Coefficient coefficient,
                    ulong index)
    {
        using var factor = BigMath.PowModQ(baseElement, index);
        using var coordinateShift = BigMath.MultModQ(coefficient.Value, factor);

        return BigMath.AddModQ(initialState, coordinateShift);
    }

    public ElectionPublicKey ShareKey()
    {
        return _electionKeys.Share();
    }

    private void SaveGuardianKey(ElectionPublicKey key)
    {
        if (_otherGuardianPublicKeys is not null)
        {
            _otherGuardianPublicKeys[key.OwnerId] = key;
        }
    }

    // fromPrivateRecord
    public static implicit operator GuardianRecord(Guardian data)
    {
        var key = data._electionKeys.Share();
        return new(
            key.OwnerId,
            key.SequenceOrder,
            key.Key,
            key.CoefficientCommitments,
            key.CoefficientProofs);
    }
    public GuardianRecord Publish()
    {
        return this;
    }

    // export_private_data
    public static implicit operator GuardianPrivateRecord(Guardian data)
    {
        return new(
            data.GuardianId,
            data._electionKeys,
            data.BackupsToShare,
            data._otherGuardianPublicKeys,
            data._otherGuardianPartialKeyBackups,
            data._otherGuardianPartialKeyVerification);
    }

    public GuardianPrivateRecord ExportPrivateData()
    {
        return this;
    }


    // Set_ceremonoy_details
    public void SetCeremonyDetails(
        int numberOfGuardians,
        int quorum,
        string keyCeremonyId)
    {
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);
    }

    // decrypt_backup
    public ElementModQ? DecryptBackup(ElectionPartialKeyBackup backup)
    {
        return DecryptBackup(backup, _electionKeys);
    }

    /// <summary>
    /// Decrypts a compensated partial decryption of an elgamal encryption on behalf of a missing guardian
    /// </summary>
    /// <param name="guardianBackup">Missing guardian's backup</param>
    /// <param name="keyPair">The present guardian's key pair that will be used to decrypt the backup</param>
    /// <returns>coordinatedata of the decryption and its proof</returns>
    public ElementModQ? DecryptBackup(ElectionPartialKeyBackup guardianBackup, ElectionKeyPair keyPair)
    {
        var encryptionSeed = GetBackupSeed(
            keyPair.OwnerId,
            keyPair.SequenceOrder
        );

        var bytesOptional = guardianBackup.EncryptedCoordinate?.Decrypt(
            keyPair.KeyPair.SecretKey, encryptionSeed, false);

        if (bytesOptional is null)
            return null;

        var coordinateData = new ElementModQ(bytesOptional);
        return coordinateData;
    }


    // all_guardian_keys_received
    public bool AllGuardianKeysReceived()
    {
        return _otherGuardianPublicKeys?.Count == CeremonyDetails.NumberOfGuardians;
    }

    // generate_election_partial_key_backups
    public bool GenerateElectionPartialKeyBackups()
    {
        if (_otherGuardianPublicKeys is null)
        {
            return false;
        }

        foreach (var guardianKey in _otherGuardianPublicKeys)
        {
            var backup = GenerateElectionPartialKeyBackup(GuardianId, _electionKeys.Polynomial, guardianKey.Value);
            BackupsToShare[guardianKey.Key] = backup;
        }
        return true;
    }

    // share_election_partial_key_backup
    public ElectionPartialKeyBackup? ShareElectionPartialKeyBackup(string designatedId)
    {
        BackupsToShare.TryGetValue(designatedId, out var ret);
        return ret;
    }

    // share_election_partial_key_backups
    public List<ElectionPartialKeyBackup> ShareElectionPartialKeyBackups()
    {
        return BackupsToShare.Values.ToList();
    }

    // save_election_partial_key_backup
    public void SaveElectionPartialKeyBackup(ElectionPartialKeyBackup backup)
    {
        if (_otherGuardianPartialKeyBackups is not null)
        {
            _otherGuardianPartialKeyBackups[backup.OwnerId!] = backup;
        }
    }

    // all_election_partial_key_backups_received
    public bool AllElectionPartialKeyBackupsReceived()
    {
        return _otherGuardianPartialKeyBackups?.Count == CeremonyDetails.NumberOfGuardians - 1;
    }


    // verify_election_partial_key_backup
    public ElectionPartialKeyVerification? VerifyElectionPartialKeyBackup(string guardianId)
    {
        var backup = _otherGuardianPartialKeyBackups?[guardianId];
        var publicKey = _otherGuardianPublicKeys?[guardianId];
        if (backup is null)
        {
            return null;
        }
        if (publicKey is null)
        {
            return null;
        }
        return VerifyElectionPartialKeyBackup(guardianId, backup, publicKey, _electionKeys);
    }

    private ElectionPartialKeyVerification VerifyElectionPartialKeyBackup(string receiverGuardianId, ElectionPartialKeyBackup? senderGuardianBackup, ElectionPublicKey? senderGuardianPublicKey, ElectionKeyPair receiverGuardianKeys)
    {
        using var encryptionSeed = GetBackupSeed(
                receiverGuardianId,
                senderGuardianBackup?.DesignatedSequenceOrder
            );

        var secretKey = receiverGuardianKeys.KeyPair.SecretKey;
        var data = senderGuardianBackup?.EncryptedCoordinate?.Decrypt(
                secretKey, encryptionSeed, false);

        using var coordinateData = new ElementModQ(data);

        var verified = VerifyPolynomialCoordinate(
                coordinateData,
                senderGuardianBackup!.DesignatedSequenceOrder,
                senderGuardianPublicKey!.CoefficientCommitments
            );
        return new ElectionPartialKeyVerification()
        {
            OwnerId = senderGuardianBackup.OwnerId,
            DesignatedId = senderGuardianBackup.DesignedId,
            VerifierId = receiverGuardianId,
            Verified = verified
        };
    }

    // publish_election_backup_challenge
    public ElectionPartialKeyChallenge? PublishElectionBackupChallenge(string guardianId)
    {
        BackupsToShare.TryGetValue(guardianId, out var backup);
        if (backup is null)
            return null;
        return GenerateElectionPartialKeyChallenge(backup, _electionKeys.Polynomial);
    }

    private ElectionPartialKeyChallenge GenerateElectionPartialKeyChallenge(ElectionPartialKeyBackup backup, ElectionPolynomial polynomial)
    {
        return new ElectionPartialKeyChallenge()
        {
            OwnerId = backup.OwnerId,
            DesignatedId = backup.DesignedId,
            DesignatedSequenceOrder = backup.DesignatedSequenceOrder,
            Value = ComputePolynomialCoordinate(backup.DesignatedSequenceOrder, polynomial),
            CoefficientCommitments = polynomial.GetCommitments(),
            CoefficientProofs = polynomial.GetProofs()
        };
    }

    // verify_election_partial_key_challenge
    public ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(ElectionPartialKeyChallenge challenge)
    {
        return VerifyElectionPartialKeyChallenge(GuardianId, challenge);
    }

    private ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(string verifierId, ElectionPartialKeyChallenge challenge)
    {
        return new ElectionPartialKeyVerification()
        {
            OwnerId = challenge.OwnerId,
            DesignatedId = challenge.DesignatedId,
            VerifierId = verifierId,
            Verified = VerifyPolynomialCoordinate(
                challenge.Value,
                challenge.DesignatedSequenceOrder,
                challenge.CoefficientCommitments)
        };
    }

    private bool VerifyPolynomialCoordinate(ElementModQ? coordinate, ulong exponentModifier, List<ElementModP>? commitments)
    {
        using var commitmentOutput = Constants.ONE_MOD_P;
        foreach (var (commitment, i) in commitments!.WithIndex())
        {
            using var exponent = BigMath.PowModP(exponentModifier, i);
            using var factor = BigMath.PowModP(commitment, exponent);
            commitmentOutput.MultModP(factor);
        }
        using var valueOutput = BigMath.GPowP(coordinate);
        return valueOutput.Equals(commitmentOutput);
    }

    // save_election_partial_key_verification
    public void SaveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        if (_otherGuardianPartialKeyVerification is not null)
        {
            _otherGuardianPartialKeyVerification[verification.DesignatedId!] = verification;
        }
    }

    // all_election_partial_key_backups_verified
    public bool AllElectionPartialKeyBackupsVerified()
    {
        var required = CeremonyDetails.NumberOfGuardians - 1;
        if (_otherGuardianPartialKeyVerification?.Count != required)
            return false;
        foreach (var verification in _otherGuardianPartialKeyVerification.Values)
        {
            if (verification.Verified is false)
                return false;
        }
        return true;
    }

    // publish_joint_key
    public ElementModP? PublishJointKey()
    {
        if (AllGuardianKeysReceived() is false)
            return null;
        if (AllElectionPartialKeyBackupsVerified() is false)
            return null;

        return ElgamalCombinePublicKeys();
    }

    private ElementModP ElgamalCombinePublicKeys()
    {
        ElementModP combinedKey = Constants.ONE_MOD_P;
        foreach (var item in _otherGuardianPublicKeys!.Values)
        {
            combinedKey.MultModP(item.Key);
        }
        return combinedKey;
    }

    // share_other_guardian_key
    public ElectionPublicKey? ShareOtherGuardianKey(string guardianId)
    {
        return _otherGuardianPublicKeys?[guardianId];
    }


    /// <summary>
    /// Saves the guardian to the local storage device
    /// </summary>
    public void Save()
    {
        var storage = StorageService.GetInstance();

        GuardianPrivateRecord data = this;
        var dataJson = JsonSerializer.Serialize(data);

        var filename = GUARDIAN_PREFIX + data.GuardianId + GUARDIAN_EXT;
        var filePath = Path.Combine(PRIVATE_KEY_FOLDER, CeremonyDetails.KeyCeremonyId);

        storage.ToFile(filePath, filename, dataJson);
    }

    /// <summary>
    /// Loads the guardian from local storage device
    /// </summary>
    /// <param name="guardianId">guardian id</param>
    /// <param name="keyCeremonyId">id for the key ceremony</param>
    /// <param name="guardianCount">count of guardians</param>
    /// <param name="quorum">minimum needed number of guardians</param>
    /// <returns></returns>
    public static Guardian? Load(string guardianId, string keyCeremonyId, int guardianCount, int quorum)
    {
        var storage = StorageService.GetInstance();

        var filename = GUARDIAN_PREFIX + guardianId + GUARDIAN_EXT;
        var filePath = Path.Combine(PRIVATE_KEY_FOLDER, keyCeremonyId, filename);

        var data = storage.FromFile(filePath);
        var privateGuardian = JsonSerializer.Deserialize<GuardianPrivateRecord>(data);
        
        return privateGuardian != null ? 
            Guardian.FromPrivateRecord(privateGuardian, keyCeremonyId, guardianCount, quorum) : 
            null;
    }

    // compute_tally_share
    // public DecryptionShare? ComputeTallyShare(CiphertextTally tally, CiphertextElectionContext context)
    // {
    //     /*
    //     Compute the decryption share of tally.

    //     :param tally: Ciphertext tally to get share of
    //     :param context: Election context
    //     :return: Decryption share of tally or None if failure
    //     */
    //     return ComputeDecryptionShare(_electionKeys, tally, context);
    // }


    // compute_ballot_shares
    // compute_compensated_tally_share
    // compute_compensated_ballot_shares
    // get_valid_ballot_shares

}
