using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Proofs;
using ElectionGuard.UI.Lib.Extensions;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using System.Text.Json;

namespace ElectionGuard.ElectionSetup;


public record GuardianPrivateRecord : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ElectionKeyPair ElectionKeys { get; init; }

    public Dictionary<string, ElectionPartialKeyBackup>? BackupsToShare { get; init; }

    public Dictionary<string, ElectionPublicKey>? GuardianElectionPublicKeys { get; init; }

    public Dictionary<string, ElectionPartialKeyBackup>? GuardianElectionPartialKeyBackups { get; init; }

    public Dictionary<string, ElectionPartialKeyVerification>? GuardianElectionPartialKeyVerifications { get; init; }

    public GuardianPrivateRecord(
        string guardianId,
        ElectionKeyPair electionKeys,
        Dictionary<string, ElectionPartialKeyBackup>? backupsToShare,
        Dictionary<string, ElectionPublicKey>? guardianElectionPublicKeys,
        Dictionary<string, ElectionPartialKeyBackup>? guardianElectionPartialKeyBackups,
        Dictionary<string, ElectionPartialKeyVerification>? guardianElectionPartialKeyVerifications)
    {
        GuardianId = guardianId;
        ElectionKeys = electionKeys;
        BackupsToShare = backupsToShare;
        GuardianElectionPublicKeys = guardianElectionPublicKeys;
        GuardianElectionPartialKeyBackups = guardianElectionPartialKeyBackups;
        GuardianElectionPartialKeyVerifications = guardianElectionPartialKeyVerifications;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        ElectionKeys.Dispose();
        BackupsToShare?.Dispose();
        GuardianElectionPublicKeys?.Dispose();
        GuardianElectionPartialKeyBackups?.Dispose();
    }

}

public record GuardianRecord : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ulong SequenceOrder { get; init; }

    public ElementModP ElectionPublicKey { get; init; }

    public List<ElementModP> ElectionCommitments { get; init; }

    public List<SchnorrProof> ElectionProofs { get; init; }

    public GuardianRecord(
        string guardianId,
        ulong sequenceOrder,
        ElementModP electionPublicKey,
        List<ElementModP> electionCommitments,
        List<SchnorrProof> electionProofs)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        ElectionPublicKey = electionPublicKey;
        ElectionCommitments = electionCommitments;
        ElectionProofs = electionProofs;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        ElectionPublicKey.Dispose();
        ElectionCommitments.Dispose();
        ElectionProofs.Dispose();
    }
}

/// <summary>
/// Guardian of election responsible for safeguarding information and decrypting results.
///
/// The first half of the guardian involves the key exchange known as the key ceremony.
/// The second half relates to the decryption process.
/// </summary>
public class Guardian : DisposableBase
{
    internal const string GuardianPrefix = "guardian_";
    internal const string PrivateKeyFolder = "gui_private_keys";
    internal const string GuardianExt = ".json";

    private readonly ElectionKeyPair _electionKeys;
    private Dictionary<string, ElectionPublicKey>? _otherGuardianPublicKeys = new();
    private Dictionary<string, ElectionPartialKeyBackup>? _otherGuardianPartialKeyBackups = new();
    private Dictionary<string, ElectionPartialKeyVerification>? _otherGuardianPartialKeyVerification = new();

    public string GuardianId { get; set; }
    public ulong SequenceOrder { get; set; }
    public CeremonyDetails CeremonyDetails { get; set; }
    public Dictionary<string, ElectionPartialKeyBackup> BackupsToShare { get; set; } = new();

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    public Guardian(
        string guardianId,
        ulong sequenceOrder,
        int numberOfGuardians,
        int quorum,
        string keyCeremonyId)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;

        _electionKeys = new(guardianId, sequenceOrder, quorum);
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        SaveGuardianKey(_electionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    public Guardian(
        string guardianId,
        ulong sequenceOrder,
        CeremonyDetails ceremonyDetails,
        ElementModQ secretKey)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;

        var keyPair = new ElGamalKeyPair(secretKey);
        _electionKeys = new(guardianId, sequenceOrder, ceremonyDetails.Quorum, keyPair);
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_electionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    public Guardian(
        string guardianId,
        ulong sequenceOrder,
        CeremonyDetails ceremonyDetails,
        ElGamalKeyPair elGamalKeyPair)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;

        _electionKeys = new(guardianId, sequenceOrder, ceremonyDetails.Quorum, elGamalKeyPair);
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_electionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails
        )
    {
        _electionKeys = keyPair;
        GuardianId = keyPair.OwnerId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_electionKeys.Share());
    }

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

        SaveGuardianKey(_electionKeys.Share());
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
    private ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(string senderGuardianId, ElectionPolynomial electionPolynomial, ElectionPublicKey receiverGuardianPublicKey)
    {
        var coordinate = electionPolynomial.ComputeCoordinate(receiverGuardianPublicKey.SequenceOrder);
        using var nonce = BigMath.RandQ();
        var seed = GetBackupSeed(
                receiverGuardianPublicKey.OwnerId,
                receiverGuardianPublicKey.SequenceOrder);

        var data = coordinate.ToBytes();
        var encryptedCoordinate = HashedElgamal.Encrypt(data, (ulong)data.Length, nonce, receiverGuardianPublicKey.Key, seed);

        return new()
        {
            OwnerId = senderGuardianId,
            DesignatedId = receiverGuardianPublicKey.OwnerId,
            DesignatedSequenceOrder = receiverGuardianPublicKey.SequenceOrder,
            EncryptedCoordinate = encryptedCoordinate
        };
    }

    private ElementModQ GetBackupSeed(string ownerId, ulong? sequenceOrder)
    {
        return BigMath.HashElems(ownerId, sequenceOrder ?? 0);
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

    public void SaveGuardianKey(ElectionPublicKey key)
    {
        if (_otherGuardianPublicKeys is null)
        {
            _otherGuardianPublicKeys = new();
        }

        _otherGuardianPublicKeys[key.OwnerId] = key;
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
            _otherGuardianPublicKeys = new();
        }

        foreach (var guardianKey in _otherGuardianPublicKeys.Values)
        {
            var backup = GenerateElectionPartialKeyBackup(GuardianId, _electionKeys.Polynomial, guardianKey);
            BackupsToShare[guardianKey.OwnerId] = backup;
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
        if (_otherGuardianPartialKeyBackups is null)
        {
            _otherGuardianPartialKeyBackups = new();
        }
        _otherGuardianPartialKeyBackups[backup.OwnerId!] = backup;
    }

    // all_election_partial_key_backups_received
    public bool AllElectionPartialKeyBackupsReceived()
    {
        return _otherGuardianPartialKeyBackups?.Count == CeremonyDetails.NumberOfGuardians - 1;
    }


    // verify_election_partial_key_backup
    public ElectionPartialKeyVerification? VerifyElectionPartialKeyBackup(string guardianId, string keyCeremonyId)
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
        return VerifyElectionPartialKeyBackup(backup?.DesignatedId!, backup, publicKey, _electionKeys, keyCeremonyId);
    }

    private ElectionPartialKeyVerification VerifyElectionPartialKeyBackup(
        string receiverGuardianId,
        ElectionPartialKeyBackup? senderGuardianBackup,
        ElectionPublicKey? senderGuardianPublicKey,
        ElectionKeyPair electionKeys, string keyCeremonyId)
    {
        using var encryptionSeed = GetBackupSeed(
                receiverGuardianId,
                senderGuardianBackup?.DesignatedSequenceOrder
            );

        var secretKey = electionKeys.KeyPair.SecretKey;
        var data = senderGuardianBackup?.EncryptedCoordinate?.Decrypt(
                secretKey, encryptionSeed, false);

        using var coordinateData = new ElementModQ(data);

        var verified = ElectionPolynomial.VerifyCoordinate(
                senderGuardianBackup!.DesignatedSequenceOrder,
                coordinateData,
                senderGuardianPublicKey!.CoefficientCommitments
            );
        return new()
        {
            KeyCeremonyId = keyCeremonyId,
            OwnerId = senderGuardianBackup.OwnerId,
            DesignatedId = senderGuardianBackup.DesignatedId,
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

    private static ElectionPartialKeyChallenge GenerateElectionPartialKeyChallenge(
        ElectionPartialKeyBackup backup, ElectionPolynomial polynomial)
    {
        return new()
        {
            OwnerId = backup.OwnerId,
            DesignatedId = backup.DesignatedId,
            DesignatedSequenceOrder = backup.DesignatedSequenceOrder,
            Value = polynomial.ComputeCoordinate(backup.DesignatedSequenceOrder),
            CoefficientCommitments = polynomial.Commitments,
            CoefficientProofs = polynomial.Proofs
        };
    }

    // verify_election_partial_key_challenge
    public ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(ElectionPartialKeyChallenge challenge)
    {
        return VerifyElectionPartialKeyChallenge(GuardianId, challenge);
    }

    private static ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(
        string verifierId, ElectionPartialKeyChallenge challenge)
    {
        return new ElectionPartialKeyVerification()
        {
            OwnerId = challenge.OwnerId,
            DesignatedId = challenge.DesignatedId,
            VerifierId = verifierId,
            Verified = ElectionPolynomial.VerifyCoordinate(
                challenge.DesignatedSequenceOrder,
                challenge.Value!,
                challenge.CoefficientCommitments!)
        };
    }

    // save_election_partial_key_verification
    public void SaveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        if (_otherGuardianPartialKeyVerification is null)
        {
            _otherGuardianPartialKeyVerification = new();
        }
        _otherGuardianPartialKeyVerification[verification.DesignatedId!] = verification;
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
        var combinedKey = Constants.ONE_MOD_P;
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

public static class GuardianStorageExtensions
{
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

        var filename = Guardian.GuardianPrefix + guardianId + Guardian.GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, Guardian.PrivateKeyFolder, keyCeremonyId, filename);

        var data = storage.FromFile(filePath);
        try
        {
            var privateGuardian = JsonSerializer.Deserialize<GuardianPrivateRecord>(data);
            return privateGuardian != null ?
                Guardian.FromPrivateRecord(privateGuardian, keyCeremonyId, guardianCount, quorum) :
                null;
        }
        catch (Exception ex)
        {
            throw new ElectionGuardException("Could not load guardian", ex);
        }
    }

    public static Guardian? Load(string guardianId, KeyCeremonyRecord keyCeremony)
    {
        return Load(guardianId, keyCeremony.KeyCeremonyId!, keyCeremony.NumberOfGuardians, keyCeremony.Quorum);
    }

    /// <summary>
    /// Saves the guardian to the local storage device
    /// </summary>
    public static void Save(this Guardian self, string keyCeremonyId)
    {
        var storage = StorageService.GetInstance();

        GuardianPrivateRecord data = self;
        var dataJson = JsonSerializer.Serialize(data);

        var filename = Guardian.GuardianPrefix + data.GuardianId + Guardian.GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, Guardian.PrivateKeyFolder, keyCeremonyId);

        storage.ToFile(filePath, filename, dataJson);
    }
}
