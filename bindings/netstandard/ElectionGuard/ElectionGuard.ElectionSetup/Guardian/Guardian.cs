using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Proofs;
using ElectionGuard.UI.Lib.Extensions;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// A guardian is a person or entity that is responsible for decrypting a share of the election record.
/// The GuardianPrivateRecord is a record of the private information that a guardian needs to decrypt.
///
/// A guardian participates in a key exchange ceremony where each guardian for an election shares part
/// of their private key with the other guardians.  The guardians then combine their public keys to
/// create a joint public key.  The joint public key is used to encrypt the election record.
///
/// At the end of the election the guardian then decrypts their share of the election record using
/// their share of the joint private key private key and submits proof of their partial decryption
/// for use in decrypting the election record.
/// </summary>
/// <remarks>
/// The Guardian class is responsible for managing the state of the key exchange ceremony and
/// the decryption process.
/// </remarks>
public partial class Guardian : DisposableBase
{
    private readonly ElectionKeyPair _electionKeys;
    private ElementModQ? _partialElectionSecretKey;
    private readonly Dictionary<string, ElectionPublicKey> _otherGuardianPublicKeys = new();
    private readonly Dictionary<string, ElectionPartialKeyBackup> _otherGuardianPartialKeyBackups = new();
    private readonly Dictionary<string, ElectionPartialKeyVerification> _otherGuardianPartialKeyVerification = new();

    /// <summary>
    /// The unique identifier for the guardian.
    /// </summary>
    public string GuardianId { get; private set; }

    /// <summary>
    /// The sequence order of the guardian in the key ceremony.
    /// </summary>
    public ulong SequenceOrder { get; private set; }

    /// <summary>
    /// The key ceremony details that this guardian is participating in.
    /// </summary>
    public CeremonyDetails CeremonyDetails { get; private set; }

    /// <summary>
    /// Encrypted segments of the guardian's private key that are shared with other guardians.
    /// </summary>
    public Dictionary<string, ElectionPartialKeyBackup> BackupsToShare { get; set; } = new();

    #region Constructors

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

    public Guardian(
        string guardianId,
        ulong sequenceOrder,
        int numberOfGuardians,
        int quorum,
        string keyCeremonyId,
        Random random)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;

        var keyPair = new ElGamalKeyPair(random.NextElementModQ());
        _electionKeys = new(guardianId, sequenceOrder, quorum, keyPair, random);
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        Console.WriteLine($"Created Guardian {guardianId} with secret key {keyPair.SecretKey}");

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
        _electionKeys = new(keyPair);
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
    /// <param name="backupsToShare"></param>
    /// <param name="guardianElectionPartialKeyVerifications"></param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails,
        Dictionary<string, ElectionPublicKey>? otherGuardianPublicKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherGuardianPartialKeyBackups = null,
        Dictionary<string, ElectionPartialKeyBackup>? backupsToShare = null,
        Dictionary<string, ElectionPartialKeyVerification>? guardianElectionPartialKeyVerifications = null
        )
    {
        _electionKeys = new(keyPair);
        GuardianId = keyPair.OwnerId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        if (otherGuardianPublicKeys != null)
        {
            _otherGuardianPublicKeys = otherGuardianPublicKeys;
        }

        if (otherGuardianPartialKeyBackups != null)
        {
            _otherGuardianPartialKeyBackups = otherGuardianPartialKeyBackups;
        }

        if (backupsToShare != null)
        {
            BackupsToShare = backupsToShare;
        }

        if (guardianElectionPartialKeyVerifications != null)
        {
            _otherGuardianPartialKeyVerification = guardianElectionPartialKeyVerifications;
        }

        SaveGuardianKey(_electionKeys.Share());
    }

    #endregion

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        _electionKeys?.Dispose();
        _partialElectionSecretKey?.Dispose();
        _otherGuardianPublicKeys?.Dispose();
        _otherGuardianPartialKeyBackups?.Dispose();

        BackupsToShare.Dispose();
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

    // all_guardian_keys_received
    public bool AllGuardianKeysReceived => _otherGuardianPublicKeys?.Count == CeremonyDetails.NumberOfGuardians;

    /// <summary>
    /// Shares the public key of the guardian.
    /// </summary>
    public ElectionPublicKey SharePublicKey()
    {
        return _electionKeys.Share();
    }

    public void SaveGuardianKey(ElectionPublicKey key)
    {
        if (!key.Key.IsInBounds())
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }
        _otherGuardianPublicKeys[key.OwnerId] = new(key);
    }

    // generate_election_partial_key_backups
    public bool GenerateElectionPartialKeyBackups()
    {
        if (!AllGuardianKeysReceived)
        {
            throw new InvalidOperationException("Not all guardian keys have been received.");
        }
        foreach (var guardianKey in _otherGuardianPublicKeys.Values)
        {
            var backup = GenerateElectionPartialKeyBackup(
                GuardianId,
                _electionKeys.Polynomial,
                guardianKey);
            BackupsToShare[guardianKey.OwnerId] = new(backup);
        }
        return true;
    }

    // share_election_partial_key_backups
    public List<ElectionPartialKeyBackup> ShareElectionPartialKeyBackups()
    {
        return BackupsToShare.Values.ToList();
    }

    // save_election_partial_key_backup
    public void SaveElectionPartialKeyBackup(
        ElectionPartialKeyBackup backup)
    {
        _otherGuardianPartialKeyBackups[backup.OwnerId!] = new(backup);
    }

    private static ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(
        string myGuardianId,
        ElectionPolynomial myPolynomial,
        ElectionPublicKey recipientPublicKey)
    {
        Console.WriteLine($"GenerateElectionPartialKeyBackup: {myGuardianId} -> {recipientPublicKey.OwnerId} {recipientPublicKey.SequenceOrder}");

        if (!recipientPublicKey.Key.IsInBounds())
        {
            throw new ArgumentNullException(nameof(recipientPublicKey));
        }

        var coordinate = myPolynomial.ComputeCoordinate(
            recipientPublicKey.SequenceOrder);
        var seed = GetBackupSeed(
                recipientPublicKey.OwnerId,
                recipientPublicKey.SequenceOrder);

        using var nonce = BigMath.RandQ();
        var encryptedCoordinate = HashedElgamal.Encrypt(
            coordinate, nonce, recipientPublicKey.Key, seed);

        return new(
            myGuardianId,
            recipientPublicKey.OwnerId,
            recipientPublicKey.SequenceOrder,
            encryptedCoordinate
        );
    }

    private static ElementModQ GetBackupSeed(string ownerId, ulong sequenceOrder)
    {
        return BigMath.HashElems(ownerId, sequenceOrder);
    }
}
