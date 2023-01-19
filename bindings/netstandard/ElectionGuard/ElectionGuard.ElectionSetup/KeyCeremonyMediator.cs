﻿using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.ElectionSetup;

public record GuardianPair(string OwnerId, string DesignatedId);

/// <summary>
/// The state of the verifications of all guardian election partial key backups
/// </summary>
public record BackupVerificationState(bool AllSent = false, bool AllVerified = false, List<GuardianPair>? FailedVerification = null);

/// <summary>
/// The Election joint key
/// </summary>
public class ElectionJointKey : DisposableBase
{
    /// <summary>
    /// The product of the guardian public keys
    /// K = ∏ ni=1 Ki mod p.
    /// </summary>
    public ElementModP? JointPublicKey { get; init; }

    /// <summary>
    /// The hash of the commitments that the guardians make to each other
    /// H = H(K 1,0 , K 2,0 ... , K n,0 )
    /// </summary>
    public ElementModQ? CommitmentHash { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        JointPublicKey?.Dispose();
        CommitmentHash?.Dispose();
    }
}


/// <summary>
/// KeyCeremonyMediator for assisting communication between guardians
/// </summary>
public class KeyCeremonyMediator : DisposableBase
{
    public KeyCeremonyMediator(string mediatorId, CeremonyDetails ceremonyDetails)
    {
        Id = mediatorId;
        CeremonyDetails = ceremonyDetails;
    }

    public string Id { get; }
    public CeremonyDetails CeremonyDetails { get; internal set; }

    // From Guardians
    // Round 1
    private readonly Dictionary<string, ElectionPublicKey> _electionPublicKeys = new();

    // Round 2
    private readonly Dictionary<GuardianPair, ElectionPartialKeyBackup> _electionPartialKeyBackups = new();

    // Round 3
    private readonly Dictionary<GuardianPair, ElectionPartialKeyVerification> _electionPartialKeyVerification = new();

    private readonly Dictionary<GuardianPair, ElectionPartialKeyChallenge> _electionPartialKeyChallenges = new();


    public void Announce(ElectionPublicKey shareKey)
    {
        ReceiveElectionPublicKey(shareKey);
    }

    public bool AllGuardiansAnnounced()
    {
        /* """
            Check the annoucement of all the guardians expected
            :return: True if all guardians in attendance are announced
        """ */
        return this._electionPublicKeys.Count == this.CeremonyDetails.NumberOfGuardians;
    }

    public List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        /* """
            When all guardians have announced, share their public keys indicating their announcement
        """ */
        if (AllGuardiansAnnounced() is false)
            return null;

        List<ElectionPublicKey> guardianKeys = new();
        var keys = _electionPublicKeys.Where(k => k.Key != requestingGuardianId).Select(k => k.Value);
        guardianKeys.AddRange(keys);

        return guardianKeys;
    }


    // Round 2
    public void ReceiveBackups(List<ElectionPartialKeyBackup> backups)
    {
        /* """
        Receive all the election partial key backups generated by a guardian
        """ */
        if (AllGuardiansAnnounced() is false)
            return;
        foreach (var backup in backups)
        {
            ReceiveElectionPartialKeyBackup(backup);
        }
    }

    private void ReceiveElectionPartialKeyBackup(ElectionPartialKeyBackup backup)
    {
        /* """
            Receive election partial key backup from guardian
            :param backup: Election partial key backup
            :return: boolean indicating success or failure
        """ */
        if (backup.OwnerId == backup.DesignedId)
            return;
        _electionPartialKeyBackups[new GuardianPair(backup.OwnerId!, backup.DesignedId!)] = backup;
    }

    public bool AllBackupsAvailable()
    {
        /* """
            Check the availability of all the guardians backups
            :return: True if all guardians have sent backups
        """ */
        return AllGuardiansAnnounced() && AllElectionPartialKeyBackupsAvailable();
    }

    private bool AllElectionPartialKeyBackupsAvailable()
    {
        /* """
            True if all election partial key backups for all guardians available
            :return: All election partial key backups for all guardians available
        """ */
        var requiredBackupsPerGuardian = CeremonyDetails.NumberOfGuardians - 1;
        return _electionPartialKeyBackups.Count == requiredBackupsPerGuardian * CeremonyDetails.NumberOfGuardians;
    }

    public List<ElectionPartialKeyBackup>? ShareBackups(string? requestingGuardianId)
    {
        /* """
            Share all backups designated for a specific guardian
        """ */
        if (AllGuardiansAnnounced() == false || AllBackupsAvailable() == false)
            return null;

        if (requestingGuardianId is null)
            return _electionPartialKeyBackups.Values.ToList();

        return ShareElectionPartialKeyBackupsToGuardian(requestingGuardianId);
    }

    private IEnumerable<string> GetAnnouncedGuardians()
    {
        return _electionPublicKeys.Keys;
    }

    private List<ElectionPartialKeyBackup> ShareElectionPartialKeyBackupsToGuardian(string guardianId)
    {
        /* """
            Share all election partial key backups for designated guardian
            :param guardian_id: Recipients guardian id
            :return: List of guardians designated backups
        """ */
        List<ElectionPartialKeyBackup> backups = new();
        var announcedGuardians = GetAnnouncedGuardians();
        var others = announcedGuardians.Where(g => g != guardianId);
        foreach (var currentGuardianId in others)
        {
            _electionPartialKeyBackups.TryGetValue(new GuardianPair(currentGuardianId, guardianId), out var backup);
            if (backup is not null)
                backups.Add(backup);
        }
        return backups;
    }


    // ROUND 3: Share verifications of backups
    public void ReceiveBackupVerifications(List<ElectionPartialKeyVerification> verifications)
    {
        /* """
            Receive all the election partial key verifications performed by a guardian
        """ */
        if (AllBackupsAvailable() == false)
            return;
        foreach (var verification in verifications)
        {
            ReceiveElectionPartialKeyVerification(verification);
        }
    }

    // Partial Key Verifications
    private void ReceiveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        /* """
            Receive election partial key verification from guardian
            :param verification: Election partial key verification
        """ */
        if (verification.OwnerId == verification.DesignatedId)
            return;
        _electionPartialKeyVerification[new GuardianPair(verification.OwnerId!, verification.DesignatedId!)] = verification;
    }


    public BackupVerificationState GetVerificationState()
    {
        if (AllBackupsAvailable() is false || AllElectionPartialKeyVerificationsReceived() is false)
            return new BackupVerificationState();

        return CheckVerificationOfElectionPartialKeyBackups();
    }

    private BackupVerificationState CheckVerificationOfElectionPartialKeyBackups()
    {
        /* """
            True if all election partial key backups verified
            :return: All election partial key backups verified
        """ */
        if (AllElectionPartialKeyVerificationsReceived() is false)
            return new BackupVerificationState();
        List<GuardianPair> failedVerifications = new();

        var unverified = _electionPartialKeyVerification.Values.Where(v => v.Verified is false);

        foreach (var verification in unverified)
        {
            failedVerifications.Add(new GuardianPair(verification.OwnerId!, verification.DesignatedId!));
        }

        return new BackupVerificationState(true, failedVerifications.Count == 0, failedVerifications);
    }


    public bool AllBackupsVerified()
    {
        return GetVerificationState().AllVerified;
    }

    private bool AllElectionPartialKeyVerificationsReceived()
    {
        /* """
        True if all election partial key verifications recieved
        :return: All election partial key verifications received
        """ */
        var requiredVerificationsPerGuardian = CeremonyDetails.NumberOfGuardians - 1;
        return _electionPartialKeyVerification.Count == (requiredVerificationsPerGuardian
            * CeremonyDetails.NumberOfGuardians);
    }

    // ROUND 4 (Optional): If a verification fails, guardian must issue challenge
    public ElectionPartialKeyVerification VerifyChallenge(ElectionPartialKeyChallenge challenge)
    {
        /* """
            Mediator receives challenge and will act to mediate and verify
        """ */
        var verification = VerifyElectionPartialKeyChallenge(Id, challenge);
        if (verification.Verified)
        {
            ReceiveElectionPartialKeyVerification(verification);
        }
        return verification;
    }
    private ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(
        string verifierId, ElectionPartialKeyChallenge challenge)
    {
        /* """
            Verify a challenge to a previous verification of a partial key backup
            :param verifier_id: Verifier of the challenge
            :param challenge: Election partial key challenge
            :return: Election partial key verification
        """ */
        return new ElectionPartialKeyVerification()
        {
            OwnerId = challenge.OwnerId,
            DesignatedId = challenge.DesignatedId,
            VerifierId = verifierId,
            Verified = VerifyPolynomialCoordinate(
                challenge.Value!,
                challenge.DesignatedSequenceOrder,
                challenge.CoefficientCommitments!
            )
        };
    }

    private bool VerifyPolynomialCoordinate(
        ElementModQ coordinate,
        ulong exponentModifier,
        List<ElementModP> commitments)
    {
        /* """
            Verify a polynomial coordinate value is in fact on the polynomial's curve

            :param coordinate: Value to be checked
            :param exponent_modifier: Unique modifier (usually sequence order) for exponent
            :param commitments: Public commitments for coefficients of polynomial
            :return: True if verified on polynomial
        """ */

        using var commitmentOutput = Constants.ONE_MOD_P;
        foreach (var (commitment, index) in commitments.WithIndex())
        {
            using var exponent = BigMath.PowModP(exponentModifier, index);
            using var factor = BigMath.PowModP(commitment, exponent);
            commitmentOutput.MultModP(factor);
        }
        using var valueOutput = BigMath.GPowP(coordinate);
        return valueOutput.Equals(commitmentOutput);
    }


    // FINAL: Publish joint public election key
    public ElectionJointKey? PublishJointKey()
    {
        /* """
            Publish joint election key from the public keys of all guardians
            :return: Joint key for election
        """ */
        if (AllBackupsVerified() is false)
            return null;

        return CombineElectionPublicKeys(_electionPublicKeys.Values.ToList());
    }

    private ElectionJointKey CombineElectionPublicKeys(List<ElectionPublicKey> electionPublicKeys)
    {
        /* """
            Creates a joint election key from the public keys of all guardians
            :param election_public_keys: all public keys of the guardians
            :return: ElectionJointKey for election
        """ */

        var publicKeys = electionPublicKeys.Select(k => k.Key).ToList();
        List<ElementModP> commitments = new();
        foreach (var key in electionPublicKeys)
        {
            foreach (var item in key.CoefficientCommitments)
            {
                commitments.Add(item);
            }
        }

        return new ElectionJointKey()
        {
            JointPublicKey = ElgamalCombinePublicKeys(publicKeys),
            CommitmentHash = HashElems(commitments)
        };  // H(K 1,0 , K 2,0 ... , K n,0 )
    }

    private ElementModP ElgamalCombinePublicKeys(List<ElementModP> keys)
    {
        /* """
            Combine multiple elgamal public keys into a joint key

            :param keys: list of public elgamal keys
            :return: joint key of elgamal keys
        """ */
        var product = Constants.ONE_MOD_P;
        product.MultModP(keys);
        return product;
    }

    private ElementModQ HashElems(List<ElementModP> keys)
    {
        return BigMath.HashElems(keys);
    }

    public void Reset(CeremonyDetails ceremonyDetails)
    {
        /* """
            Reset mediator to initial state
            :param ceremony_details: Ceremony details of election
        """ */
        CeremonyDetails = ceremonyDetails;
        _electionPublicKeys.Clear();
        _electionPartialKeyBackups.Clear();
        _electionPartialKeyChallenges.Clear();
        _electionPartialKeyVerification.Clear();
    }

    /// <summary>
    /// Receive election public key from guardian
    /// </summary>
    /// <param name="publicKey">Election public key</param>
    private void ReceiveElectionPublicKey(ElectionPublicKey publicKey)
    {
        _electionPublicKeys[publicKey.OwnerId] = publicKey;
    }

    public void RunStep1(string keyCeremonyId, string userId)
    {
        /* - guardian side
            key_ceremony_id = key_ceremony.id
            user_id = self._auth_service.get_required_user_id()
            self._key_ceremony_service.append_guardian_joined(db, key_ceremony_id, user_id)
            # refresh key ceremony to get the list of guardians with the authoritative order they joined in
            key_ceremony = self._key_ceremony_service.get(db, key_ceremony_id)
            guardian_number = get_guardian_number(key_ceremony, user_id)
            self.log.debug(
                f"user {user_id} about to join key ceremony {key_ceremony_id} as guardian #{guardian_number}"
            )
            guardian = make_guardian(user_id, guardian_number, key_ceremony)
            self._guardian_service.save_guardian(guardian, key_ceremony)
            public_key = guardian.share_key()
            self._key_ceremony_service.append_key(db, key_ceremony_id, public_key)
            self.log.debug(
                f"{user_id} joined key ceremony {key_ceremony_id} as guardian #{guardian_number}"
            )
            self._key_ceremony_service.notify_changed(db, key_ceremony_id)

         */

        var currentGuardianUserName = userId;

        // append guadian joined to key ceremony (db)

        // get guardian number

        // make guardian
        //var guardian = Guardian.FromNonce(currentGuardianUserName!, 0, KeyCeremony!.NumberOfGuardians, KeyCeremony.Quorum, KeyCeremony.KeyCeremonyId!);

        // save guardian to local drive / yubikey

        // get public key
        //var public_key = guardian.ShareKey();

        // append to key ceremony (db)

        // notify change to admin (signalR)
    }

    public void RunStep2()
    {
        /* - admin side
            def should_run(
                self, key_ceremony: KeyCeremonyDto, state: KeyCeremonyStates
            ) -> bool:
                is_admin = self._auth_service.is_admin()
                should_run: bool = is_admin and state == KeyCeremonyStates.PendingAdminAnnounce
                return should_run
         * 
         * 
            def run(self, db: Database, key_ceremony: KeyCeremonyDto) -> None:
                key_ceremony_id = key_ceremony.id
                self.log.info("all guardians have joined, announcing guardians")
                other_keys = self.announce(key_ceremony)
                self.log.debug("saving other_keys")
                self._key_ceremony_service.append_other_key(db, key_ceremony_id, other_keys)
                self._key_ceremony_service.notify_changed(db, key_ceremony_id)

            def announce(self, key_ceremony: KeyCeremonyDto) -> List[dict[str, Any]]:
                other_keys = []
                mediator = make_mediator(key_ceremony)
                announce_guardians(key_ceremony, mediator)
                for guardian_id in key_ceremony.guardians_joined:
                    self.log.debug(f"announcing guardian {guardian_id}")
                    other_guardian_keys: List[ElectionPublicKey] = get_optional(
                        mediator.share_announced(guardian_id)
                    )
                    other_keys.append(
                        {
                    "owner_id": guardian_id,
                            "other_keys": [
                                public_key_to_dict(key) for key in other_guardian_keys
                            ],
                        }
                    )
                return other_keys

                def announce_guardians(key_ceremony: KeyCeremonyDto, mediator: KeyCeremonyMediator
                    ) -> None:
                for guardian_id in key_ceremony.guardians_joined:
                    key = key_ceremony.find_key(guardian_id)
                    mediator.announce(key)

        */
    }

    public void RunStep3()
    {
        /* - guardian side
            def should_run(
                self, key_ceremony: KeyCeremonyDto, state: KeyCeremonyStates
            ) -> bool:
                is_guardian = not self._auth_service.is_admin()
                current_user_id = self._auth_service.get_required_user_id()
                current_user_backups = key_ceremony.get_backup_count_for_user(current_user_id)
                current_user_backup_exists = current_user_backups > 0
                return (
                    is_guardian
                    and state == KeyCeremonyStates.PendingGuardianBackups
                    and not current_user_backup_exists
                )
         * 
         * 
            def run(self, db: Database, key_ceremony: KeyCeremonyDto) -> None:
                current_user_id = self._auth_service.get_required_user_id()
                key_ceremony_id = key_ceremony.id
                self.log.debug(f"creating backups for guardian {current_user_id}")
                guardian = self._guardian_service.load_guardian_from_key_ceremony(
                    current_user_id, key_ceremony
                )
                self._guardian_service.load_other_keys(key_ceremony, current_user_id, guardian)
                guardian.generate_election_partial_key_backups()
                backups = guardian.share_election_partial_key_backups()
                self._key_ceremony_service.append_backups(db, key_ceremony_id, backups)
                # notify the admin that a new guardian has backups
                self._key_ceremony_service.notify_changed(db, key_ceremony_id)
         */
    }
    public void RunStep4()
    {
        /* - admin side
            def should_run(
                self, key_ceremony: KeyCeremonyDto, state: KeyCeremonyStates
            ) -> bool:
                is_admin: bool = self._auth_service.is_admin()
                return is_admin and state == KeyCeremonyStates.PendingAdminToShareBackups
         * 
         * 
            def run(self, db: Database, key_ceremony: KeyCeremonyDto) -> None:
                current_user_id = self._auth_service.get_user_id()
                self.log.debug(f"sharing backups for admin {current_user_id}")
                shared_backups = self.share_backups(key_ceremony)
                self._key_ceremony_service.append_shared_backups(
                    db, key_ceremony.id, shared_backups
                )
                self._key_ceremony_service.notify_changed(db, key_ceremony.id)

            def share_backups(self, key_ceremony: KeyCeremonyDto) -> List[Any]:
                mediator = make_mediator(key_ceremony)
                announce_guardians(key_ceremony, mediator)
                mediator.receive_backups(key_ceremony.get_backups())
                shared_backups = []
                for guardian_id in key_ceremony.guardians_joined:
                    self.log.debug(f"sharing backups for guardian {guardian_id}")
                    guardian_backups = mediator.share_backups(guardian_id)
                    if guardian_backups is None:
                        raise Exception("Error sharing backups")
                    backups_as_dict = [backup_to_dict(backup) for backup in guardian_backups]
                    shared_backups.append({"owner_id": guardian_id, "backups": backups_as_dict})
                return shared_backups
         */
    }

    public void RunStep5()
    {
        /* - guardian side
            def should_run(
                self, key_ceremony: KeyCeremonyDto, state: KeyCeremonyStates
            ) -> bool:
                is_guardian = not self._auth_service.is_admin()
                current_user_id = self._auth_service.get_required_user_id()
                current_user_verifications = key_ceremony.get_verification_count_for_user(
                    current_user_id
                )
                current_user_verification_exists = current_user_verifications > 0
                return (
                    is_guardian
                    and state == KeyCeremonyStates.PendingGuardiansVerifyBackups
                    and not current_user_verification_exists
                )
         * 
         * 
            def run(self, db: Database, key_ceremony: KeyCeremonyDto) -> None:
                current_user_id = self._auth_service.get_required_user_id()
                shared_backups = key_ceremony.get_shared_backups_for_guardian(current_user_id)
                guardian = self._guardian_service.load_guardian_from_key_ceremony(
                    current_user_id, key_ceremony
                )
                self._guardian_service.load_other_keys(key_ceremony, current_user_id, guardian)
                verifications: List[ElectionPartialKeyVerification] = []
                for backup in shared_backups:
                    self.log.debug(
                        f"verifying backup from {backup.owner_id} to {current_user_id}"
                    )
                    guardian.save_election_partial_key_backup(backup)
                    verification = guardian.verify_election_partial_key_backup(backup.owner_id)
                    if verification is None:
                        raise Exception("Error verifying backup")
                    verifications.append(verification)
                self._key_ceremony_service.append_verifications(
                    db, key_ceremony.id, verifications
                )
                # notify the admin that a new verification was created
                self._key_ceremony_service.notify_changed(db, key_ceremony.id)
         */
    }

    public void RunStep6()
    {
        /* - admin side
            def should_run(
                self, key_ceremony: KeyCeremonyDto, state: KeyCeremonyStates
            ) -> bool:
                is_admin = self._auth_service.is_admin()
                return is_admin and state == KeyCeremonyStates.PendingAdminToPublishJointKey
         * 
         * 
            def run(self, db: Database, key_ceremony: KeyCeremonyDto) -> None:
                current_user_id = self._auth_service.get_user_id()
                self.log.debug(f"receiving verifications for admin {current_user_id}")
                mediator = make_mediator(key_ceremony)
                announce_guardians(key_ceremony, mediator)
                mediator.receive_backups(key_ceremony.get_backups())
                verifications = key_ceremony.get_verifications()
                mediator.receive_backup_verifications(verifications)
                election_joint_key = mediator.publish_joint_key()
                if election_joint_key is None:
                    raise Exception("Failed to publish joint key")
                self.log.info(f"joint key published: {election_joint_key.joint_public_key}")
                self._key_ceremony_service.append_joint_key(
                    db, key_ceremony.id, election_joint_key
                )
                self._key_ceremony_service.set_complete(db, key_ceremony.id)
                # notify everyone that verifications completed and the joint key published
                self._key_ceremony_service.notify_changed(db, key_ceremony.id)
         */
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        foreach (var item in _electionPublicKeys)
        {
            item.Value.Dispose();
        }

        foreach (var item in _electionPartialKeyBackups)
        {
            item.Value.Dispose();
        }

        foreach (var item in _electionPartialKeyChallenges)
        {
            item.Value.Dispose();
        }
    }

}

