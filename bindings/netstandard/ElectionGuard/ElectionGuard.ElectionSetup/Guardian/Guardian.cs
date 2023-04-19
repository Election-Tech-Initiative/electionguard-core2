using System.Diagnostics;
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
    private readonly ElectionKeyPair _myElectionKeys;
    private ElementModQ? _myPartialSecretKey;
    private readonly Dictionary<string, ElectionPublicKey> _publicKeys = new();
    private readonly Dictionary<string, ElectionPartialKeyBackup> _partialKeyBackups = new();
    private readonly Dictionary<string, ElectionPartialKeyVerification> _partialVerifications = new();

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

        _myElectionKeys = new(guardianId, sequenceOrder, quorum);
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        SaveGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    // Do not use this constructor in production. It is only for testing.
    /// </summary>
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

        using var nextRandom = random.NextElementModQ();
        var keyPair = new ElGamalKeyPair(nextRandom);
        _myElectionKeys = new(guardianId, sequenceOrder, quorum, keyPair, random);
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        SaveGuardianKey(_myElectionKeys.Share());
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
        _myElectionKeys = new(guardianId, sequenceOrder, ceremonyDetails.Quorum, keyPair);
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_myElectionKeys.Share());
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

        _myElectionKeys = new(guardianId, sequenceOrder, ceremonyDetails.Quorum, elGamalKeyPair);
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_myElectionKeys.Share());
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
        _myElectionKeys = new(keyPair);
        GuardianId = keyPair.OwnerId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        SaveGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    /// <param name="otherKeys">The public keys the guardian generated during a key ceremony</param>
    /// <param name="otherBackups">The partial key backups the guardian generated during a key ceremony</param>
    /// <param name="backupsToShare"></param>
    /// <param name="otherVerifications"></param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails,
        Dictionary<string, ElectionPublicKey>? otherKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherBackups = null,
        Dictionary<string, ElectionPartialKeyBackup>? backupsToShare = null,
        Dictionary<string, ElectionPartialKeyVerification>? otherVerifications = null
        )
    {
        _myElectionKeys = new(keyPair);
        GuardianId = keyPair.OwnerId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        _publicKeys = otherKeys
            ?? _publicKeys;
        _partialKeyBackups = otherBackups
            ?? _partialKeyBackups;
        BackupsToShare = backupsToShare
            ?? BackupsToShare;
        _partialVerifications = otherVerifications
            ?? _partialVerifications;

        SaveGuardianKey(_myElectionKeys.Share());
    }

    #endregion

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        _myElectionKeys?.Dispose();
        _myPartialSecretKey?.Dispose();
        _publicKeys?.Dispose();
        _partialKeyBackups?.Dispose();

        BackupsToShare?.Dispose();
    }

    // export_private_data
    public static implicit operator GuardianPrivateRecord(Guardian data)
    {
        return new(data.GuardianId, data._myElectionKeys);
    }

    // all_guardian_keys_received
    public bool AllGuardianKeysReceived => _publicKeys?.Count == CeremonyDetails.NumberOfGuardians;

    /// <summary>
    /// Shares the public key of the guardian.
    /// </summary>
    public ElectionPublicKey SharePublicKey()
    {
        return _myElectionKeys.Share();
    }

    public void SaveGuardianKey(ElectionPublicKey key)
    {
        if (!key.Key.IsAddressable)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }
        _publicKeys[key.OwnerId] = new(key);
    }

    // generate_election_partial_key_backups
    public bool GenerateElectionPartialKeyBackups()
    {
        if (!AllGuardianKeysReceived)
        {
            throw new InvalidOperationException("Not all guardian keys have been received.");
        }
        foreach (var guardianKey in _publicKeys.Values)
        {
            var backup = GenerateElectionPartialKeyBackup(
                GuardianId,
                _myElectionKeys.Polynomial,
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
        _partialKeyBackups[backup.OwnerId!] = new(backup);
    }

    private static ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(
        string myGuardianId,
        ElectionPolynomial myPolynomial,
        ElectionPublicKey recipientPublicKey)
    {
        Debug.WriteLine($"GenerateElectionPartialKeyBackup: {myGuardianId} -> {recipientPublicKey.OwnerId} {recipientPublicKey.SequenceOrder}");

        if (!recipientPublicKey.Key.IsAddressable)
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
