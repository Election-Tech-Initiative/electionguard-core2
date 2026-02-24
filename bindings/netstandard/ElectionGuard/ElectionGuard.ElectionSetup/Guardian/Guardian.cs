using System.Diagnostics;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;
using ElectionGuard.ElectionSetup.Records;

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
public partial class Guardian : DisposableBase, IElectionGuardian
{
    private readonly ElectionKeyPair _myElectionKeys;
    private readonly ElementModQ _commitmentSeed;
    private ElementModQ? _myPartialSecretKey;
    private readonly Dictionary<string, ElectionPublicKey> _publicKeys = new();
    private readonly Dictionary<string, ElectionPartialKeyBackupRecord> _partialKeyBackups = new();
    private readonly Dictionary<string, ElectionPartialKeyVerificationRecord> _partialVerifications = new();

    public string ObjectId => GuardianId;

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
    public Dictionary<string, ElectionPartialKeyBackupRecord> BackupsToShare { get; set; } = new();

    private ElementModP? _commitmentOffset;

    /// <summary>
    /// The commitment offset for the polynomial.
    /// This value is computed as the inner most product of the following equation
    ///
    /// Π Π 𝐾j^𝑖^m mod 𝑝 in the spec Equation (63)
    /// 
    /// This value can be computed from public data for all guardians
    /// and is used to regenerate commitments during decryption.
    /// It is calculated during the key ceremony process
    /// to simplify the decryption process.
    /// </summary>
    public ElementModP? CommitmentOffset
    {
        get
        {
            // once we receive all of the keys
            // we can calculate the commitment offset
            if (_commitmentOffset == null && AllGuardianKeysReceived)
            {
                _commitmentOffset = _publicKeys.ComputeCommitmentOffset(_myElectionKeys.SequenceOrder);
            }
            return _commitmentOffset;
        }
    }


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
        _commitmentSeed = BigMath.RandQ();
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        AddGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    // Do not use this constructor in production. It is only for testing.
    /// </summary>
    [Obsolete("Do not use this constructor in production. It is only for testing.")]
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
        _commitmentSeed = keyPair.SecretKey;
        CeremonyDetails = new(keyCeremonyId, numberOfGuardians, quorum);

        AddGuardianKey(_myElectionKeys.Share());
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
        _commitmentSeed = BigMath.RandQ();
        CeremonyDetails = ceremonyDetails;

        AddGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    public Guardian(
        string guardianId,
        ulong sequenceOrder,
        CeremonyDetails ceremonyDetails,
        ElGamalKeyPair elGamalKeyPair,
        ElementModQ commitmentSeed)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;

        _myElectionKeys = new(guardianId, sequenceOrder, ceremonyDetails.Quorum, elGamalKeyPair);
        _commitmentSeed = new(commitmentSeed);
        CeremonyDetails = ceremonyDetails;

        AddGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    /// <param name="commitmentSeed">A secret value used by the guardian to create commitments for generating proofs during decryption</param>
    public Guardian(
        ElectionKeyPair keyPair,
        CeremonyDetails ceremonyDetails,
        ElementModQ commitmentSeed
        )
    {
        _myElectionKeys = new(keyPair);
        _commitmentSeed = new(commitmentSeed);
        GuardianId = keyPair.GuardianId;
        SequenceOrder = keyPair.SequenceOrder;
        CeremonyDetails = ceremonyDetails;

        AddGuardianKey(_myElectionKeys.Share());
    }

    /// <summary>
    /// Initialize a guardian with the specified arguments.
    /// </summary>
    /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
    /// <param name="commitmentSeed">A secret value used by the guardian to create commitments for generating proofs during decryption</param>
    /// <param name="ceremonyDetails">The details of the key ceremony</param>
    /// <param name="otherKeys">The public keys the guardian generated during a key ceremony</param>
    /// <param name="otherBackups">The partial key backups the guardian generated during a key ceremony</param>
    /// <param name="backupsToShare"></param>
    /// <param name="otherVerifications"></param>
    public Guardian(
        ElectionKeyPair keyPair,
        ElementModQ commitmentSeed,
        CeremonyDetails ceremonyDetails,
        Dictionary<string, ElectionPublicKey>? otherKeys = null,
        Dictionary<string, ElectionPartialKeyBackupRecord>? otherBackups = null,
        Dictionary<string, ElectionPartialKeyBackupRecord>? backupsToShare = null,
        Dictionary<string, ElectionPartialKeyVerificationRecord>? otherVerifications = null
        )
    {
        _myElectionKeys = new(keyPair);
        _commitmentSeed = new(commitmentSeed);
        GuardianId = keyPair.GuardianId;
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

        AddGuardianKey(_myElectionKeys.Share());
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
        return new(data.GuardianId, data._myElectionKeys, data._commitmentSeed);
    }

    /// <summary>
    /// Returns true if the number of public keys is equal to the number of guardians.
    /// </summary>
    public bool AllGuardianKeysReceived => _publicKeys?.Count == CeremonyDetails.NumberOfGuardians;

    /// <summary>
    /// Shares the public key of the guardian.
    /// </summary>
    public ElectionPublicKey SharePublicKey()
    {
        return _myElectionKeys.Share();
    }

    [Obsolete("Used for testing. Do not use in production.")]
    public ElementModQ GetSecretKey()
    {
        return _myElectionKeys.KeyPair.SecretKey;
    }

    /// <summary>
    /// Adds the public keys of guardians to the list of public keys.
    /// </summary>
    public void AddGuardianKeys(List<ElectionPublicKey> keys)
    {
        foreach (var key in keys)
        {
            AddGuardianKey(key);
        }
    }

    /// <summary>
    /// Adds the public key of a guardian to the list of public keys.
    /// </summary>
    public void AddGuardianKey(ElectionPublicKey key)
    {
        if (!key.Key.IsAddressable)
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }
        _publicKeys[key.GuardianId] = new(key);

        // if we have received all keys
        // compute the commitment offset
        if (AllGuardianKeysReceived)
        {
            // access the commitment offset variable to force the computation
            var _ = CommitmentOffset;
        }
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
                SequenceOrder,
                _myElectionKeys.Polynomial,
                guardianKey);
            BackupsToShare[guardianKey.GuardianId] = new(backup.OwnerId, backup.OwnerSequenceOrder, backup.DesignatedId, backup.DesignatedSequenceOrder, backup.EncryptedCoordinate);
        }
        return true;
    }

    // share_election_partial_key_backups
    public List<ElectionPartialKeyBackupRecord> ShareElectionPartialKeyBackups()
    {
        return BackupsToShare.Values.ToList();
    }

    // save_election_partial_key_backup
    public void SaveElectionPartialKeyBackup(
        ElectionPartialKeyBackupRecord backup)
    {
        _partialKeyBackups[backup.OwnerId!] = new(backup.OwnerId, backup.OwnerSequenceOrder, backup.DesignatedId, backup.DesignatedSequenceOrder, backup.EncryptedCoordinate);
    }

    private static ElectionPartialKeyBackupRecord GenerateElectionPartialKeyBackup(
        string myGuardianId,
        ulong mySequenceOrder,
        ElectionPolynomial myPolynomial,
        ElectionPublicKey recipientPublicKey)
    {
        Debug.WriteLine($"GenerateElectionPartialKeyBackup: {myGuardianId} -> {recipientPublicKey.GuardianId} {recipientPublicKey.SequenceOrder}");

        if (!recipientPublicKey.Key.IsAddressable)
        {
            throw new ArgumentNullException(nameof(recipientPublicKey));
        }

        var coordinate = myPolynomial.ComputeCoordinate(
            recipientPublicKey.SequenceOrder);
        var seed = CreateBackupSeed(
                mySequenceOrder,
                recipientPublicKey.SequenceOrder);

        using var nonce = BigMath.RandQ();
        var encryptedCoordinate = HashedElgamal.Encrypt(
            coordinate, nonce, Hash.Prefix_GuardianShareSecret, recipientPublicKey.Key, seed);

        return new(
            myGuardianId,
            mySequenceOrder,
            recipientPublicKey.GuardianId,
            recipientPublicKey.SequenceOrder,
            encryptedCoordinate
        );
    }

    /// <summary>
    /// hashes together the guardian sequence orders to create an encryption seed
    /// </summary>
    private static ElementModQ CreateBackupSeed(ulong mySequenceOrder, ulong theirSequenceOrder)
    {
        return Hash.HashElems(mySequenceOrder, theirSequenceOrder);
    }

    /// <summary>
    /// hashes together the guardian sequence orders to re-create an encryption seed
    /// </summary>
    private static ElementModQ GetBackupSeed(ulong mySequenceOrder, ulong theirSequenceOrder)
    {
        return Hash.HashElems(theirSequenceOrder, mySequenceOrder);
    }
}
