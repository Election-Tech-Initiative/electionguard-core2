using ElectionGuard.ElectionSetup.Exceptions;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Bson;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// KeyCeremonyMediator for assisting communication between guardians
/// </summary>
public class KeyCeremonyMediator : DisposableBase
{
    public KeyCeremonyMediator(string mediatorId, string userId, KeyCeremony keyCeremony)
    {
        Id = mediatorId;
        UserId = userId;
        CeremonyDetails = keyCeremony;
        _keyCeremony = keyCeremony;
        CreateAdminSteps();
        CreateGuardianSteps();
    }

    private List<KeyCeremonyStep> _adminSteps = new();
    private List<KeyCeremonyStep> _guardianSteps = new();

    public string UserId { get; }
    public string Id { get; }
    public CeremonyDetails CeremonyDetails { get; internal set; }
    public KeyCeremony _keyCeremony { get; internal set; }

    private object mutex = new();

    // From Guardians
    // Round 1
    private readonly Dictionary<string, ElectionPublicKey> _electionPublicKeys = new();

    // Round 2
    private readonly Dictionary<GuardianPair, ElectionPartialKeyBackup> _electionPartialKeyBackups = new();

    // Round 3
    private readonly Dictionary<GuardianPair, ElectionPartialKeyVerification> _electionPartialKeyVerification = new();

    private readonly Dictionary<GuardianPair, ElectionPartialKeyChallenge> _electionPartialKeyChallenges = new();

    private void CreateAdminSteps()
    {
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardiansJoin,
                RunStep = RunStep2,
                ShouldRunStep = ShouldAdminStartStep2
            });
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingAdminAnnounce,
                RunStep = RunStep2,
                ShouldRunStep = AlwaysRun
            });
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardianBackups,
                RunStep = RunStep4,
                ShouldRunStep = ShouldAdminStartStep4
            });
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingAdminToShareBackups,
                RunStep = RunStep4,
                ShouldRunStep = AlwaysRun
            });
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardiansVerifyBackups,
                RunStep = RunStep6,
                ShouldRunStep = ShouldAdminStartStep6
            });
        _adminSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingAdminToPublishJointKey,
                RunStep = RunStep6,
                ShouldRunStep = AlwaysRun
            });
    }

    private void CreateGuardianSteps()
    {
        _guardianSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardiansJoin,
                RunStep = RunStep1,
                ShouldRunStep = ShouldGuardianRunStep1
            });
        _guardianSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardianBackups,
                RunStep = RunStep3,
                ShouldRunStep = ShouldGuardianRunStep3
            });
        _guardianSteps.Add(
            new KeyCeremonyStep()
            {
                State = KeyCeremonyState.PendingGuardiansVerifyBackups,
                RunStep = RunStep5,
                ShouldRunStep = ShouldGuardianRunStep5
            });
    }

    public void Announce(ElectionPublicKey shareKey)
    {
        ReceiveElectionPublicKey(shareKey);
    }

    /// <summary>
    /// Check the annoucement of all the guardians expected
    /// </summary>
    /// <returns>True if all guardians in attendance are announced</returns>
    public bool AllGuardiansAnnounced()
    {
        return _electionPublicKeys.Count == CeremonyDetails.NumberOfGuardians;
    }

    /// <summary>
    /// When all guardians have announced, share their public keys indicating their announcement
    /// </summary>
    /// <param name="requestingGuardianId"></param>
    /// <returns></returns>
    private List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        if (AllGuardiansAnnounced() is false)
            return null;

        List<ElectionPublicKey> guardianKeys = new();
        var keys = _electionPublicKeys.Where(k => k.Key != requestingGuardianId).Select(k => k.Value);
        guardianKeys.AddRange(keys);

        return guardianKeys;
    }


    // Round 2
    /// <summary>
    /// Receive all the election partial key backups generated by a guardian
    /// </summary>
    /// <param name="backups"></param>
    public void ReceiveBackups(List<GuardianBackups> backups)
    {
        if (AllGuardiansAnnounced() is false)
            return;

        foreach (var backup in backups)
        {
            ReceiveElectionPartialKeyBackup(backup.Backup!);
        }
    }

    /// <summary>
    /// Receive election partial key backup from guardian
    /// </summary>
    /// <param name="backup">Election partial key backup</param>
    private void ReceiveElectionPartialKeyBackup(ElectionPartialKeyBackup backup)
    {
        if (backup.OwnerId == backup.DesignatedId)
            return;
        _electionPartialKeyBackups[new GuardianPair(backup.OwnerId!, backup.DesignatedId!)] = backup;
    }

    /// <summary>
    /// Check the availability of all the guardians backups
    /// </summary>
    /// <returns>True if all guardians have sent backups</returns>
    public bool AllBackupsAvailable()
    {
        return AllGuardiansAnnounced() && AllElectionPartialKeyBackupsAvailable();
    }

    /// <summary>
    /// True if all election partial key backups for all guardians available
    /// </summary>
    /// <returns>All election partial key backups for all guardians available</returns>
    private bool AllElectionPartialKeyBackupsAvailable()
    {
        var requiredBackupsPerGuardian = CeremonyDetails.NumberOfGuardians - 1;
        return _electionPartialKeyBackups.Count == requiredBackupsPerGuardian * CeremonyDetails.NumberOfGuardians;
    }

    /// <summary>
    /// Share all backups designated for a specific guardian
    /// </summary>
    /// <param name="requestingGuardianId"></param>
    /// <returns></returns>
    public List<ElectionPartialKeyBackup>? ShareBackups(string? requestingGuardianId)
    {
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

    /// <summary>
    /// Share all election partial key backups for designated guardian
    /// </summary>
    /// <param name="guardianId">Recipients guardian id</param>
    /// <returns>List of guardians designated backups</returns>
    private List<ElectionPartialKeyBackup> ShareElectionPartialKeyBackupsToGuardian(string guardianId)
    {
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
    /// <summary>
    /// Receive all the election partial key verifications performed by a guardian
    /// </summary>
    /// <param name="verifications"></param>
    public void ReceiveBackupVerifications(List<ElectionPartialKeyVerification> verifications)
    {
        if (AllBackupsAvailable() == false)
            return;
        foreach (var verification in verifications)
        {
            ReceiveElectionPartialKeyVerification(verification);
        }
    }

    // Partial Key Verifications
    /// <summary>
    /// Receive election partial key verification from guardian
    /// </summary>
    /// <param name="verification">Election partial key verification</param>
    private void ReceiveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        _electionPartialKeyVerification[new GuardianPair(verification.OwnerId!, verification.DesignatedId!)] = verification;
    }


    public BackupVerificationState GetVerificationState()
    {
        if (AllBackupsAvailable() is false || AllElectionPartialKeyVerificationsReceived() is false)
            return new BackupVerificationState();

        return CheckVerificationOfElectionPartialKeyBackups();
    }

    /// <summary>
    /// True if all election partial key backups verified
    /// </summary>
    /// <returns>All election partial key backups verified</returns>
    private BackupVerificationState CheckVerificationOfElectionPartialKeyBackups()
    {
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

    /// <summary>
    /// True if all election partial key verifications recieved
    /// </summary>
    /// <returns>All election partial key verifications received</returns>
    private bool AllElectionPartialKeyVerificationsReceived()
    {
        var requiredVerificationsPerGuardian = CeremonyDetails.NumberOfGuardians;
        return _electionPartialKeyVerification.Count == (requiredVerificationsPerGuardian
            * CeremonyDetails.NumberOfGuardians);
    }

    // ROUND 4 (Optional): If a verification fails, guardian must issue challenge
    /// <summary>
    /// Mediator receives challenge and will act to mediate and verify
    /// </summary>
    /// <param name="challenge"></param>
    /// <returns></returns>
    public ElectionPartialKeyVerification VerifyChallenge(ElectionPartialKeyChallenge challenge)
    {
        var verification = VerifyElectionPartialKeyChallenge(Id, challenge);
        if (verification.Verified)
        {
            ReceiveElectionPartialKeyVerification(verification);
        }
        return verification;
    }

    /// <summary>
    /// Verify a challenge to a previous verification of a partial key backup
    /// </summary>
    /// <param name="verifierId"></param>
    /// <param name="challenge"></param>
    /// <returns></returns>
    private ElectionPartialKeyVerification VerifyElectionPartialKeyChallenge(
        string verifierId, ElectionPartialKeyChallenge challenge)
    {
        /* """
            
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

    /// <summary>
    /// Verify a polynomial coordinate value is in fact on the polynomial's curve
    /// </summary>
    /// <param name="coordinate">Value to be checked</param>
    /// <param name="exponentModifier">Unique modifier (usually sequence order) for exponent</param>
    /// <param name="commitments">Public commitments for coefficients of polynomial</param>
    /// <returns>True if verified on polynomial</returns>
    private bool VerifyPolynomialCoordinate(
        ElementModQ coordinate,
        ulong exponentModifier,
        List<ElementModP> commitments)
    {
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
    /// <summary>
    /// Publish joint election key from the public keys of all guardians
    /// </summary>
    /// <returns>Joint key for election</returns>
    public ElectionJointKey? PublishJointKey()
    {
        if (AllBackupsVerified() is false)
            return null;

        return CombineElectionPublicKeys(_electionPublicKeys.Values.ToList());
    }

    /// <summary>
    /// Creates a joint election key from the public keys of all guardians
    /// </summary>
    /// <param name="electionPublicKeys">all public keys of the guardians</param>
    /// <returns>ElectionJointKey for election</returns>
    private ElectionJointKey CombineElectionPublicKeys(List<ElectionPublicKey> electionPublicKeys)
    {
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

    /// <summary>
    /// Combine multiple elgamal public keys into a joint key
    /// </summary>
    /// <param name="keys">list of public elgamal keys</param>
    /// <returns>joint key of elgamal keys</returns>
    private ElementModP ElgamalCombinePublicKeys(List<ElementModP> keys)
    {
        var product = Constants.ONE_MOD_P;
        product.MultModP(keys);
        return product;
    }

    private ElementModQ HashElems(List<ElementModP> keys)
    {
        return BigMath.HashElems(keys);
    }

    /// <summary>
    /// Reset mediator to initial state
    /// </summary>
    /// <param name="ceremonyDetails">Ceremony details of election</param>
    public void Reset(CeremonyDetails ceremonyDetails)
    {
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

    private async Task<ulong> GetGuardianNumberAsync(GuardianPublicKeyService service, string keyCeremonyId, string guardianId)
    {
        var list = await service.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        foreach ((var item, ulong index) in list.WithIndex())
        {
            if (item.GuardianId == guardianId)
            {
                return index + 1;
            }
        }
        throw new ArgumentOutOfRangeException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if a step was run</returns>
    public async Task RunKeyCeremony(bool isAdmin = false)
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        KeyCeremonyService keyCeremonyService = new();
        var keyCeremony = await keyCeremonyService.GetByKeyCeremonyIdAsync(keyCeremonyId);
        if (keyCeremony == null)
        {
            throw new KeyCeremonyException(
                KeyCeremonyState.DoesNotExist,
                keyCeremonyId,
                UserId,
                $"Key Ceremony {keyCeremonyId} does not exist");
        }
        _keyCeremony.State = keyCeremony.State;

        var state = keyCeremony.State;

        var steps = isAdmin ? _adminSteps : _guardianSteps;
        var currentStep = steps.SingleOrDefault(s => s.State == state);
        if (currentStep != null && await currentStep.ShouldRunStep!())
        {
//            if (Monitor.TryEnter(mutex))
            {
                try
                {
                    await currentStep.RunStep!();
                }
                finally
                {
//                    Monitor.Exit(mutex);
                }
            }
        }
    }

    private async Task<bool> AlwaysRun()
    {
        return await Task.FromResult(true);
    }

    private async Task<bool> ShouldAdminStartStep2()
    {
        string keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        GuardianPublicKeyService guardianService = new();
        var guardianCount = await guardianService.CountAsync(keyCeremonyId);

        return (guardianCount == CeremonyDetails.NumberOfGuardians);
    }

    private async Task<bool> ShouldAdminStartStep4()
    {
        string keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        GuardianPublicKeyService guardianService = new();
        var guardianCount = await guardianService.CountAsync(keyCeremonyId);

        GuardianBackupService backupService = new();
        var backupCount = await backupService.CountAsync(keyCeremonyId);

        return (guardianCount == CeremonyDetails.NumberOfGuardians &&
            backupCount == guardianCount * guardianCount);
    }

    private async Task<bool> ShouldAdminStartStep6()
    {
        string keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        GuardianPublicKeyService guardianService = new();
        var guardianCount = await guardianService.CountAsync(keyCeremonyId);

        GuardianBackupService backupService = new();
        var backupCount = await backupService.CountAsync(keyCeremonyId);

        VerificationService verificationService = new();
        var verificationCount = await verificationService.CountAsync(keyCeremonyId);

        var verificationList = await verificationService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        if (verificationList == null)
            return false;

        var unverifiedCount = verificationList.Count(v => v.Verified == false);

        return (guardianCount == CeremonyDetails.NumberOfGuardians &&
            backupCount == guardianCount * guardianCount &&
            verificationCount == guardianCount * guardianCount &&
            unverifiedCount == 0);
    }


    private async Task<bool> ShouldGuardianRunStep1()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;

        GuardianPublicKeyService guardianService = new();
        var guardian = await guardianService.GetByIdsAsync(keyCeremonyId, guardianId);
        var count = await guardianService.CountAsync(keyCeremonyId);

        return guardian == null && count < CeremonyDetails.NumberOfGuardians;
    }

    private async Task<bool> ShouldGuardianRunStep3()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;

        // might want to switch this to use a count instead of loading the object
        GuardianPublicKeyService guardianService = new();
        var guardian = await guardianService.GetByIdsAsync(keyCeremonyId, guardianId);
        if (guardian == null)
        {
            return false;
        }

        GuardianBackupService backupService = new();
        var backupCount = await backupService.CountAsync(keyCeremonyId, guardianId);

        return (backupCount != CeremonyDetails.NumberOfGuardians);
    }

    private async Task<bool> ShouldGuardianRunStep5()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;

        GuardianPublicKeyService guardianService = new();
        var guardian = await guardianService.GetByIdsAsync(keyCeremonyId, guardianId);
        if (guardian == null)
        {
            return false;
        }

        GuardianBackupService backupService = new();
        var backupCount = await backupService.CountAsync(keyCeremonyId, guardianId);

        VerificationService verificationService = new();
        var verificationCount = await verificationService.CountAsync(keyCeremonyId, guardianId);

        return (backupCount == CeremonyDetails.NumberOfGuardians &&
            verificationCount != CeremonyDetails.NumberOfGuardians);
    }


    private async Task RunStep1()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var currentGuardianUserName = UserId;

        // append guardian joined to key ceremony (db)
        GuardianPublicKeyService service = new();
        GuardianPublicKey data = new() { KeyCeremonyId = keyCeremonyId, GuardianId = currentGuardianUserName };
        _ = await service.SaveAsync(data);

        // get guardian number
        var guardianNumber = await GetGuardianNumberAsync(service, keyCeremonyId, currentGuardianUserName);

        // make guardian
        var guardian = Guardian.FromNonce(currentGuardianUserName, guardianNumber, CeremonyDetails.NumberOfGuardians, CeremonyDetails.Quorum, keyCeremonyId);

        // save guardian to local drive / yubikey
        guardian.Save();

        // get public key
        var publicKey = guardian.ShareKey();

        // append to key ceremony (db)
        try
        {
            await service.UpdatePublicKeyAsync(keyCeremonyId, currentGuardianUserName, publicKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        // notify change to admin (signalR)
    }

    private void AnnounceGuardians(List<GuardianPublicKey> list)
    {
        foreach (var item in list)
        {
            Announce(item.PublicKey!);
        }
    }

    private async Task Announce(string keyCeremonyId)
    {
        GuardianPublicKeyService service = new();
        var list = await service.GetAllByKeyCeremonyIdAsync(keyCeremonyId);

        AnnounceGuardians(list);
    }

    private async Task RunStep2()
    {
        // change state to step2
        string keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        KeyCeremonyService service = new();
        _keyCeremony.State = KeyCeremonyState.PendingAdminAnnounce;
        await service.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    

        // self.log.info("all guardians have joined, announcing guardians")
        // call announce
        await Announce(keyCeremonyId);

        // change state to step3
        _keyCeremony.State = KeyCeremonyState.PendingGuardianBackups;
        await service.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    

    }

    private async Task RunStep3()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        KeyCeremonyService service = new();
        var keyCeremony = await service.GetByKeyCeremonyIdAsync(keyCeremonyId);

        // load guardian from key ceremony
        var guardian = Guardian.Load(UserId, keyCeremony!);

        // load other keys
        GuardianPublicKeyService guardianService = new();
        var list = await guardianService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        foreach (var item in list)
        {
            guardian!.SaveGuardianKey(item.PublicKey!);
        }

        // generate election partial key backups
        _ = guardian!.GenerateElectionPartialKeyBackups();

        // share backups
        var backups = guardian.ShareElectionPartialKeyBackups();

        // save backups to database
        GuardianBackupService backupService = new();
        foreach (var item in backups)
        {
            GuardianBackups data = new()
            {
                KeyCeremonyId = keyCeremonyId,
                GuardianId = UserId,
                DesignatedId = item.DesignatedId,
                Backup = item
            };
            _ = await backupService.SaveAsync(data);
        }

        // notify change to admin (signalR)

    }

    private async Task RunStep4()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        // change state
        KeyCeremonyService service = new();
        await service.UpdateStateAsync(keyCeremonyId, KeyCeremonyState.PendingAdminToShareBackups);
        // notify change to guardians (signalR)    

        // Announce guardians - puts data into keyceremony mediator structures
        await Announce(keyCeremonyId);

        // get backups
        GuardianBackupService backupService = new();
        var backups = await backupService.GetByKeyCeremonyIdAsync(keyCeremonyId);

        // verify that the backups are all complete

        // receive backups
        ReceiveBackups(backups!);

        // change state
        await service.UpdateStateAsync(keyCeremonyId, KeyCeremonyState.PendingGuardiansVerifyBackups);
        // notify change to guardians (signalR)    


    }

    private async Task RunStep5()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        KeyCeremonyService keyCeremonyService = new();
        GuardianBackupService backupService = new();
        GuardianPublicKeyService publicKeyService = new();
        var backups = await backupService.GetByGuardianIdAsync(keyCeremonyId, UserId);
        var keyCeremony = await keyCeremonyService.GetByKeyCeremonyIdAsync(keyCeremonyId);
        var guardian = Guardian.Load(UserId, keyCeremony!);
        var list = await publicKeyService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        foreach (var item in list)
        {
            guardian!.SaveGuardianKey(item.PublicKey!);
        }
        List<ElectionPartialKeyVerification> verifications = new();
        foreach (var backup in backups!)
        {
            guardian!.SaveElectionPartialKeyBackup(backup.Backup!);
            var verification = guardian.VerifyElectionPartialKeyBackup(backup.GuardianId!, keyCeremonyId);
            if(verification == null)
            {
                throw new KeyCeremonyException(
                    keyCeremony!.State,
                    keyCeremonyId,
                    UserId,
                    $"Error verifying back from {backup.Backup!.OwnerId!}");
            }
            verifications.Add(verification);
        }
        // save verifications
        VerificationService verificationService = new();
        foreach (var verification in verifications)
        {
            await verificationService.SaveAsync(verification);
        }

        // notify change to admin (signalR)

    }

    private async Task RunStep6()
    {
        string keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        // change state
        KeyCeremonyService service = new();
        await service.UpdateStateAsync(keyCeremonyId, KeyCeremonyState.PendingAdminToPublishJointKey);
        // notify change to guardians (signalR)    

        // Announce guardians - puts data into keyceremony mediator structures
        await Announce(keyCeremonyId);

        // get backups
        GuardianBackupService backupService = new();
        var backups = await backupService.GetByKeyCeremonyIdAsync(keyCeremonyId);
        ReceiveBackups(backups!);

        // get all verifications
        VerificationService verificationService = new();
        var verifications = await verificationService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        // receive verifications
        ReceiveBackupVerifications(verifications!);

        // publish joint key
        var jointKey = PublishJointKey();

        // if null then throw
        if(jointKey == null)
        {
            throw new KeyCeremonyException(
                KeyCeremonyState.PendingAdminToPublishJointKey,
                keyCeremonyId,
                UserId,
                $"Failed to publish joint key for {keyCeremonyId}");
        }

        // save joint key to key ceremony
        // update state to complete
        await service.UpdateCompleteAsync(keyCeremonyId, jointKey);
        // notify change to guardians (signalR)

    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        _electionPublicKeys.Dispose();
        _electionPartialKeyBackups.Dispose();
        _electionPartialKeyChallenges.Dispose();
    }

}

