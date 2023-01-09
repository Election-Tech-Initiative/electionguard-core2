using ElectionGuard.ElectionSetup.Extensions;

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
public class Guardian
{
    private readonly ElectionKeyPair _electionKeys;
    //private readonly CeremonyDetails _ceremonyDetails;
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
        //GenerateBackupKeys();
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
        ElectionPublicKey public_key)
    {
        var el_gamal_key_pair = ElGamalKeyPair.FromPair(Constants.ZERO_MOD_Q, public_key.Key);
        var keyPair = new ElectionKeyPair(
            public_key.OwnerId,
            public_key.SequenceOrder,
            el_gamal_key_pair,
            new ElectionPolynomial(new()));
        var ceremonyDetails = new CeremonyDetails(keyCeremonyId, numberOfGuardians, quorum);
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
        var ceremonyDetails = new CeremonyDetails(keyCeremonyId, numberOfGuardians, quorum);
        return new(keyPair, ceremonyDetails);
    }



    //    private ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(ulong sequenceOrder)
    private ElectionPartialKeyBackup GenerateElectionPartialKeyBackup(string senderGuardianID, ElectionPolynomial senderGuardianPolynomial, ElectionPublicKey receiverGuardianPublicKey)
    {
        var coordinate = ComputePolynomialCoordinate((ulong)receiverGuardianPublicKey.SequenceOrder);
        var nonce = BigMath.RandQ();
        var seed = GetBackupSeed(
                receiverGuardianPublicKey.OwnerId,
                receiverGuardianPublicKey.SequenceOrder
            );

        var data = coordinate.ToBytes();
        var encryptedCoordinate = HashedElgamal.Encrypt(data, (ulong)data.Length, nonce, receiverGuardianPublicKey.Key, seed);

        return new()
        {
            OwnerId = senderGuardianID,
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
        var sequenceOrderModQ = new ElementModQ(sequenceOrder);
        var coordinate = Constants.ZERO_MOD_Q; // start at 0 mod q.

        var coefficients = polynomial != null ? polynomial.Coefficients : _electionKeys.Polynomial.Coefficients;

        foreach (var (coefficient, index) in coefficients.WithIndex())
        {
            coordinate = GetCoordinate(sequenceOrderModQ, coordinate, coefficient, index);
        }

        return coordinate;
    }

    // TODO: This is a candidate for a Math class... or a Polynomial class. This does not belong to a guardian.
    private static ElementModQ GetCoordinate(
        ElementModQ initialState,
        ElementModQ baseElement,
        Coefficient coefficient,
                    int index)
    {
        var exponent = new ElementModQ((ulong)index);
        var factor = BigMath.PowModQ(baseElement, exponent);
        var coordinateShift = BigMath.MultModQ(coefficient.Value, factor);

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
        CeremonyDetails = new CeremonyDetails(keyCeremonyId, numberOfGuardians, quorum);
    }

    // decrypt_backup
    public ElementModQ? DecryptBackup(ElectionPartialKeyBackup backup)
    {
        // TODO: finish the decrypt
        // return decrypt_backup(get_optional(backup), self._election_keys)

        return null;
    }

    // all_guardian_keys_received
    public bool AllGuardianKeysReceived()
    {
        return _otherGuardianPublicKeys.Count == CeremonyDetails.NumberOfGuardians;
    }

    // generate_election_partial_key_backups
    public bool GenerateElectionPartialKeyBackups()
    {
        foreach (var guardianKey in _otherGuardianPublicKeys)
        {
            var backup = GenerateElectionPartialKeyBackup(GuardianId, _electionKeys.Polynomial, guardianKey.Value);
            if (backup == null)
            {
                // add logging
                return false;
            }
            BackupsToShare[guardianKey.Key] = backup;
        }
        return true;
    }

    // share_election_partial_key_backup
    public ElectionPartialKeyBackup? ShareElectionPartialKeyBackup(string designatedId)
    {
        return BackupsToShare[designatedId];
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
            _otherGuardianPartialKeyBackups[backup.OwnerId] = backup;
        }
    }

    // all_election_partial_key_backups_received
    public bool AllElectionPartialKeyBackupsReceived()
    {
        return _otherGuardianPartialKeyBackups.Count == CeremonyDetails.NumberOfGuardians - 1;
    }


    // verify_election_partial_key_backup
    public ElectionPartialKeyVerification? VerifyElectionPartialKeyBackup(string guardianId)
    {
        var backup = _otherGuardianPartialKeyBackups?[guardianId];
        var publicKey = _otherGuardianPublicKeys?[guardianId];
        if (backup is null)
        {
            //raise ValueError(f"No backup exists for {guardian_id}")
        }
        if (publicKey is null)
        {
            //raise ValueError(f"No public key exists for {guardian_id}")
        }
        return VerifyElectionPartialKeyBackup(guardianId, backup, publicKey, _electionKeys);
    }

    private ElectionPartialKeyVerification VerifyElectionPartialKeyBackup(string receiverGuardianId, ElectionPartialKeyBackup? senderGuardianBackup, ElectionPublicKey? senderGuardianPublicKey, ElectionKeyPair receiverGuardianKeys)
    {
        var encryption_seed = GetBackupSeed(
                receiverGuardianId,
                senderGuardianBackup?.DesignatedSequenceOrder
            );

        var secretKey = receiverGuardianKeys.KeyPair.SecretKey;
        var data = senderGuardianBackup?.EncryptedCoordinate.Decrypt(
                secretKey, encryption_seed, false);

        var coordinateData = new ElementModQ(data);

        var verified = VerifyPolynomialCoordinate(
                coordinateData,
                senderGuardianBackup.DesignatedSequenceOrder,
                senderGuardianPublicKey.CoefficientCommitments
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
        var backup = BackupsToShare[guardianId];
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

    private bool VerifyPolynomialCoordinate(ElementModQ coordinate, ulong exponent_modifier, List<ElementModP> commitments)
    {
        var exponent_modifier_mod_q = new ElementModP(exponent_modifier);
        var commitmentOutput = Constants.ONE_MOD_P;
        foreach (var (commitment, i) in commitments.WithIndex())
        {
            var modi = new ElementModP((ulong)i);
            var exponent = BigMath.PowModP(exponent_modifier_mod_q, modi);
            var factor = BigMath.PowModP(commitment, exponent);
            commitmentOutput = BigMath.MultModP(commitmentOutput, factor);
        }
        var valueOutput = BigMath.GPowP(coordinate);
        return valueOutput == commitmentOutput;
    }

    // save_election_partial_key_verification
    public void SaveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        if (_otherGuardianPublicKeys is not null)
        {
            _otherGuardianPartialKeyVerification[verification.DesignatedId] = verification;
        }
    }

    // all_election_partial_key_backups_verified
    public bool AllElectionPartialKeyBackupsVerified()
    {
        var required = CeremonyDetails.NumberOfGuardians - 1;
        if (_otherGuardianPartialKeyVerification.Count != required)
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

        // public_keys = map(
        //     lambda public_key: public_key.key,
        //     self._guardian_election_public_keys.values(),
        // )
        // return ElgamalCombinePublicKeys(public_keys);
        return null;
    }

    // share_other_guardian_key
    public ElectionPublicKey? ShareOtherGuardianKey(string guardianId)
    {
        return _otherGuardianPublicKeys[guardianId];
    }

    // compute_tally_share
    // compute_ballot_shares
    // compute_compensated_tally_share
    // compute_compensated_ballot_shares
    // get_valid_ballot_shares

}
