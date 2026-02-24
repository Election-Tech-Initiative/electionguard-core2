using System.Diagnostics;
using ElectionGuard.UI.Lib.Exceptions;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.UI.Lib.Services;

public interface IKeyCeremonyMediator
{
    string UserId { get; }
    string Id { get; }
    CeremonyDetails CeremonyDetails { get; }
    KeyCeremonyRecord _keyCeremony { get; }

    bool AllBackupsAvailable();
    bool AllBackupsVerified();
    bool AllGuardiansAnnounced();
    void Announce(ElectionPublicKey shareKey);
    BackupVerificationState GetVerificationState();
    Task<bool> HasBackup(string guardianId);
    Task<(bool, bool)> HasVerified(string guardianId);
    ElectionJointKey? PublishJointKey();
    void ReceiveBackups(List<GuardianBackups> backups);
    void ReceiveBackupVerifications(List<ElectionPartialKeyVerification> verifications);
    void ReceiveElectionPartialKeyBackup(ElectionPartialKeyBackup backup);
    void ReceiveElectionPartialKeyVerification(ElectionPartialKeyVerification verification);
    List<ElectionPartialKeyBackup>? ShareBackups(string? requestingGuardianId = null);
    ElectionPartialKeyVerification VerifyChallenge(ElectionPartialKeyChallenge challenge);
}

public interface IKeyCeremonyMediatorStateMachine
{
    Task RunKeyCeremony(bool isAdmin = false);
    void Reset(CeremonyDetails ceremonyDetails);

    Task<bool> ShouldAdminStartStep2();
    Task<bool> ShouldAdminStartStep4();
    Task<bool> ShouldAdminStartStep6();

    Task<bool> ShouldGuardianRunStep1();
    Task<bool> ShouldGuardianRunStep3();
    Task<bool> ShouldGuardianRunStep5();

}

/// <summary>
/// KeyCeremonyMediator for assisting communication between guardians
/// </summary>
public class KeyCeremonyMediator
    : DisposableBase, IKeyCeremonyMediator, IKeyCeremonyMediatorStateMachine
{
    public KeyCeremonyMediator(
        string mediatorId,
        string userId,
        KeyCeremonyRecord keyCeremony,
        IKeyCeremonyService keyCeremonyService,
        IGuardianBackupService backupService,
        IGuardianPublicKeyService publicKeyService,
        IVerificationService verificationService)
    {
        Id = mediatorId;
        UserId = userId;
        CeremonyDetails = keyCeremony;
        _keyCeremony = keyCeremony;
        _keyCeremonyService = keyCeremonyService;
        _backupService = backupService;
        _publicKeyService = publicKeyService;
        _verificationService = verificationService;
        CreateAdminSteps();
        CreateGuardianSteps();
    }

    protected readonly IKeyCeremonyService _keyCeremonyService;
    protected readonly IGuardianBackupService _backupService;
    protected readonly IGuardianPublicKeyService _publicKeyService;
    protected readonly IVerificationService _verificationService;

    private readonly List<StateMachineStep<KeyCeremonyState>> _adminSteps = new();
    private readonly List<StateMachineStep<KeyCeremonyState>> _guardianSteps = new();

    // HACK: This only works as a mutex because we are using 5s long polling.
    // TODO: figure out a more graceful way to support long polling 
    // without multiple steps running at once.
    private static bool IsRunning = false;

    public string UserId { get; }
    public string Id { get; }
    public CeremonyDetails CeremonyDetails { get; internal set; }
    public KeyCeremonyRecord _keyCeremony { get; internal set; }

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
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingGuardiansJoin,
                RunStep = RunStep2,
                ShouldRunStep = ShouldAdminStartStep2
            });
        _adminSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingAdminAnnounce,
                RunStep = RunStep2,
                ShouldRunStep = AlwaysRun
            });
        _adminSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingGuardianBackups,
                RunStep = RunStep4,
                ShouldRunStep = ShouldAdminStartStep4
            });
        _adminSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingAdminToShareBackups,
                RunStep = RunStep4,
                ShouldRunStep = AlwaysRun
            });
        _adminSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingGuardiansVerifyBackups,
                RunStep = RunStep6,
                ShouldRunStep = ShouldAdminStartStep6
            });
        _adminSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingAdminToPublishJointKey,
                RunStep = RunStep6,
                ShouldRunStep = AlwaysRun
            });
    }

    private void CreateGuardianSteps()
    {
        _guardianSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingGuardiansJoin,
                RunStep = RunStep1,
                ShouldRunStep = ShouldGuardianRunStep1
            });
        _guardianSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
            {
                State = KeyCeremonyState.PendingGuardianBackups,
                RunStep = RunStep3,
                ShouldRunStep = ShouldGuardianRunStep3
            });
        _guardianSteps.Add(
            new StateMachineStep<KeyCeremonyState>()
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
    protected virtual List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        if (AllGuardiansAnnounced() is false)
        {
            return null;
        }

        List<ElectionPublicKey> guardianKeys = new();
        var keys = _electionPublicKeys.Where(k => k.Key != requestingGuardianId)
        .Select(k => k.Value);
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
        {
            return;
        }

        foreach (var backup in backups)
        {
            ReceiveElectionPartialKeyBackup(backup.Backup!);
        }
    }

    /// <summary>
    /// Receive election partial key backup from guardian
    /// </summary>
    /// <param name="backup">Election partial key backup</param>
    public void ReceiveElectionPartialKeyBackup(ElectionPartialKeyBackup backup)
    {
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
        return _electionPartialKeyBackups.Count == CeremonyDetails.NumberOfGuardians * CeremonyDetails.NumberOfGuardians;
    }

    /// <summary>
    /// Share all backups designated for a specific guardian
    /// </summary>
    /// <param name="requestingGuardianId"></param>
    /// <returns></returns>
    public List<ElectionPartialKeyBackup>? ShareBackups(string? requestingGuardianId = null)
    {
        if (AllGuardiansAnnounced() == false || AllBackupsAvailable() == false)
        {
            return null;
        }

        if (requestingGuardianId is null)
        {
            return _electionPartialKeyBackups.Values.ToList();
        }

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
    private List<ElectionPartialKeyBackup> ShareElectionPartialKeyBackupsToGuardian(
        string guardianId)
    {
        List<ElectionPartialKeyBackup> backups = new();
        var announcedGuardians = GetAnnouncedGuardians();
        var others = announcedGuardians;
        foreach (var currentGuardianId in others)
        {
            _ = _electionPartialKeyBackups.TryGetValue(
                new GuardianPair(currentGuardianId, guardianId), out var backup);
            if (backup is not null)
            {
                backups.Add(backup);
            }
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
        {
            return;
        }

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
    public void ReceiveElectionPartialKeyVerification(ElectionPartialKeyVerification verification)
    {
        _electionPartialKeyVerification[
            new GuardianPair(
                verification.OwnerId!, verification.DesignatedId!)] = verification;
    }


    public BackupVerificationState GetVerificationState()
    {
        if (AllBackupsAvailable() is false || AllElectionPartialKeyVerificationsReceived() is false)
        {
            return new BackupVerificationState();
        }

        return CheckVerificationOfElectionPartialKeyBackups();
    }

    /// <summary>
    /// True if all election partial key backups verified
    /// </summary>
    /// <returns>All election partial key backups verified</returns>
    private BackupVerificationState CheckVerificationOfElectionPartialKeyBackups()
    {
        if (AllElectionPartialKeyVerificationsReceived() is false)
        {
            return new BackupVerificationState();
        }

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
        var totalRequired = requiredVerificationsPerGuardian * CeremonyDetails.NumberOfGuardians;
        return _electionPartialKeyVerification.Count == totalRequired;
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
    /// <param name="verifierId">verifier of the challenge</param>
    /// <param name="challenge">election partial key challenge</param>
    /// <returns></returns>
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
                challenge.CoefficientCommitments!
            )
        };
    }

    // FINAL: Publish joint public election key
    /// <summary>
    /// Publish joint election key from the public keys of all guardians
    /// </summary>
    /// <returns>Joint key for election</returns>
    public ElectionJointKey? PublishJointKey()
    {
        return AllBackupsVerified() is false
        ? null
        : CombineElectionPublicKeys(_electionPublicKeys.Values.ToList());
    }

    /// <summary>
    /// Creates a joint election key from the public keys of all guardians
    /// </summary>
    /// <param name="electionPublicKeys">all public keys of the guardians</param>
    /// <returns>ElectionJointKey for election</returns>
    private static ElectionJointKey CombineElectionPublicKeys(List<ElectionPublicKey> electionPublicKeys)
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
            JointPublicKey = ElgamalCombinePublicKeys(publicKeys!),
            CommitmentHash = Hash.HashElems(commitments)
        };  // H(K 1,0 , K 2,0 ... , K n,0 )
    }

    /// <summary>
    /// Combine multiple elgamal public keys into a joint key
    /// </summary>
    /// <param name="keys">list of public elgamal keys</param>
    /// <returns>joint key of elgamal keys</returns>
    private static ElementModP ElgamalCombinePublicKeys(List<ElementModP> keys)
    {
        var product = Constants.ONE_MOD_P;
        _ = product.MultModP(keys);
        return product;
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
        _electionPublicKeys[publicKey.GuardianId] = publicKey;
    }

    private static async Task<ulong> GetGuardianSequenceOrderAsync(
        IGuardianPublicKeyService service, string keyCeremonyId, string guardianId)
    {
        var list = await service.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        if (list == null)
        {
            return 1;
        }

        foreach ((var item, var index) in list.WithIndex())
        {
            if (item.GuardianId == guardianId)
            {
                // the math functions need this to be a 1 based value instead of a 0 based value
                return index + 1;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(guardianId));
    }

    #region IKeyCeremonyMediatorStateMachine

    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if a step was run</returns>
    public async Task RunKeyCeremony(bool isAdmin = false)
    {
        if (!IsRunning)
        {
            var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

            var keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(keyCeremonyId);
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
                IsRunning = true;
                try
                {
                    await currentStep.RunStep!();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    Debug.WriteLine($"error running key ceremony step: {currentStep.State} {ex}");
                }
                IsRunning = false;
            }
        }
    }

    private async Task<bool> AlwaysRun()
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ShouldAdminStartStep2()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardians = await _publicKeyService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);

        return guardians.Count == CeremonyDetails.NumberOfGuardians
            && guardians.All(i => i.PublicKey != null);
    }

    public async Task<bool> ShouldAdminStartStep4()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianCount = await _publicKeyService.CountAsync(keyCeremonyId);
        var backupCount = await _backupService.CountAsync(keyCeremonyId);

        return guardianCount == CeremonyDetails.NumberOfGuardians &&
            backupCount == guardianCount * guardianCount;
    }

    public async Task<bool> ShouldAdminStartStep6()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianCount = await _publicKeyService.CountAsync(keyCeremonyId);
        var backupCount = await _backupService.CountAsync(keyCeremonyId);
        var verificationCount = await _verificationService.CountAsync(keyCeremonyId);

        var verificationList = await _verificationService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        if (verificationList == null)
        {
            return false;
        }

        var unverifiedCount = verificationList.Count(v => v.Verified == false);
        Debug.WriteLine($"guardianCount: {guardianCount} backupCount: {backupCount} verificationCount: {verificationCount} unverifiedCount: {unverifiedCount}");

        return guardianCount == CeremonyDetails.NumberOfGuardians &&
            backupCount == guardianCount * guardianCount &&
            verificationCount == guardianCount * guardianCount &&
            unverifiedCount == 0;
    }

    public async Task<bool> ShouldGuardianRunStep1()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;
        var guardian = await _publicKeyService.GetByIdsAsync(keyCeremonyId, guardianId);
        var guardianCount = await _publicKeyService.CountAsync(keyCeremonyId);
        return guardian == null && guardianCount < CeremonyDetails.NumberOfGuardians;
    }

    public async Task<bool> ShouldGuardianRunStep3()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;

        // might want to switch this to use a count instead of loading the object
        var guardian = await _publicKeyService.GetByIdsAsync(keyCeremonyId, guardianId);
        if (guardian == null)
        {
            return false;
        }

        var backupCount = await _backupService.CountAsync(keyCeremonyId, guardianId);
        return backupCount != CeremonyDetails.NumberOfGuardians;
    }

    public async Task<bool> ShouldGuardianRunStep5()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var guardianId = UserId;

        var guardian = await _publicKeyService.GetByIdsAsync(keyCeremonyId, guardianId);
        if (guardian == null)
        {
            return false;
        }

        var backupCount = await _backupService.CountAsync(keyCeremonyId, guardianId);


        var verificationCount = await _verificationService.CountAsync(keyCeremonyId, guardianId);

        return backupCount == CeremonyDetails.NumberOfGuardians &&
            verificationCount != CeremonyDetails.NumberOfGuardians;
    }

    private async Task RunStep1()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var currentGuardianUserName = UserId;

        // append guardian joined to key ceremony (db)

        using var newRecord = new GuardianPublicKey
        {
            KeyCeremonyId = keyCeremonyId,
            GuardianId = currentGuardianUserName,
            PublicKey = null
        };
        _ = await _publicKeyService.SaveAsync(newRecord);

        // get guardian number
        var sequenceOrder = await GetGuardianSequenceOrderAsync(
            _publicKeyService, keyCeremonyId, currentGuardianUserName);

        // make guardian
        var guardian = new Guardian(
            currentGuardianUserName,
            sequenceOrder,
            CeremonyDetails.NumberOfGuardians,
            CeremonyDetails.Quorum,
            keyCeremonyId);

        // save guardian to local drive / yubikey
        GuardianStorageExtensions.Save(guardian, keyCeremonyId);

        // get public key
        var publicKey = guardian.SharePublicKey();
        if (publicKey == null || !publicKey.Key.IsAddressable)
        {
            throw new Exception("Error getting public key");
        }

        // append to key ceremony (db)
        await _publicKeyService.UpdatePublicKeyAsync(
            keyCeremonyId, currentGuardianUserName, publicKey);

        // notify change to admin (signalR)
    }

    private void AnnounceGuardians(List<GuardianPublicKey> guardians)
    {
        foreach (var item in guardians)
        {
            Announce(item.PublicKey!);
        }
    }

    private async Task Announce(string keyCeremonyId)
    {
        var guardians = await _publicKeyService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);

        AnnounceGuardians(guardians);
    }

    private async Task RunStep2()
    {
        // change state to step2
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        _keyCeremony.State = KeyCeremonyState.PendingAdminAnnounce;
        await _keyCeremonyService.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    

        // call announce
        await Announce(keyCeremonyId);

        // change state to step3
        _keyCeremony.State = KeyCeremonyState.PendingGuardianBackups;
        await _keyCeremonyService.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    
    }

    private async Task RunStep3()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;
        var keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(keyCeremonyId);

        // load guardian from key ceremony
        var guardian = GuardianStorageExtensions.Load(UserId, keyCeremony!);

        // load other keys
        var publicKeys = await _publicKeyService.GetAllByKeyCeremonyIdAsync(
            keyCeremonyId);
        foreach (var publicKey in publicKeys)
        {
            guardian!.AddGuardianKey(publicKey.PublicKey!);
        }

        // generate election partial key backups
        _ = guardian!.GenerateElectionPartialKeyBackups();

        // share backups
        var backups = guardian.ShareElectionPartialKeyBackups();

        // save backups to database
        foreach (var backup in backups)
        {
            using GuardianBackups data = new(
                keyCeremonyId,
                UserId,
                backup.DesignatedId!,
                new(backup)
            );

            _ = await _backupService.SaveAsync(data);
        }

        // notify change to admin (signalR)
    }

    private async Task RunStep4()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        // change state
        _keyCeremony.State = KeyCeremonyState.PendingAdminToShareBackups;
        await _keyCeremonyService.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    

        // Announce guardians - puts data into keyceremony mediator structures
        await Announce(keyCeremonyId);

        // get backups
        var backups = await _backupService.GetByKeyCeremonyIdAsync(keyCeremonyId);

        // verify that the backups are all complete

        // receive backups
        ReceiveBackups(backups!);

        // change state
        _keyCeremony.State = KeyCeremonyState.PendingGuardiansVerifyBackups;
        await _keyCeremonyService.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    
    }

    private async Task RunStep5()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        var backups = await _backupService.GetByGuardianIdAsync(keyCeremonyId, UserId);
        var keyCeremony = await _keyCeremonyService.GetByKeyCeremonyIdAsync(keyCeremonyId);
        var guardian = GuardianStorageExtensions.Load(UserId, keyCeremony!);
        var publicKeys = await _publicKeyService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        foreach (var publicKey in publicKeys)
        {
            guardian!.AddGuardianKey(publicKey.PublicKey!);
        }
        List<ElectionPartialKeyVerification> verifications = new();
        // TODO: ISSUE #213 throw on invalid backup
        foreach (var backup in backups!)
        {
            guardian!.SaveElectionPartialKeyBackup(backup.Backup!.ToRecord());
            var verification = guardian.VerifyElectionPartialKeyBackup(backup.GuardianId!, keyCeremonyId);
            if (verification == null) // TODO: ISSUE #213 throw on invalid backup
            {
                throw new KeyCeremonyException(
                    keyCeremony!.State,
                    keyCeremonyId,
                    UserId,
                    $"Error verifying back from {backup.Backup!.OwnerId!}");
            }
            verifications.Add(new(verification));
        }
        // save verifications
        foreach (var verification in verifications)
        {
            _ = await _verificationService.SaveAsync(verification);
        }

        // notify change to admin (signalR)
    }

    private async Task RunStep6()
    {
        var keyCeremonyId = CeremonyDetails.KeyCeremonyId;

        // change state
        _keyCeremony.State = KeyCeremonyState.PendingAdminToPublishJointKey;
        await _keyCeremonyService.UpdateStateAsync(keyCeremonyId, _keyCeremony.State);
        // notify change to guardians (signalR)    

        // Announce guardians - puts data into keyceremony mediator structures
        await Announce(keyCeremonyId);

        // get backups
        var backups = await _backupService.GetByKeyCeremonyIdAsync(keyCeremonyId);
        ReceiveBackups(backups!);

        // get all verifications
        var verifications = await _verificationService.GetAllByKeyCeremonyIdAsync(keyCeremonyId);
        // receive verifications
        ReceiveBackupVerifications(verifications!);

        // publish joint key
        var jointKey = PublishJointKey();

        // if null then throw
        if (jointKey == null)
        {
            throw new KeyCeremonyException(
                KeyCeremonyState.PendingAdminToPublishJointKey,
                keyCeremonyId,
                UserId,
                $"Failed to publish joint key for {keyCeremonyId}");
        }

        // save joint key to key ceremony
        // update state to complete
        _keyCeremony.State = KeyCeremonyState.Complete;
        await _keyCeremonyService.UpdateCompleteAsync(keyCeremonyId, jointKey);
        // notify change to guardians (signalR)
    }

    #endregion

    public async Task<bool> HasBackup(string guardianId)
    {
        var backups = await _backupService.GetByKeyCeremonyIdAsync(CeremonyDetails.KeyCeremonyId);
        var list = backups?.Where(b => b.GuardianId == guardianId);
        return list?.Count() > 0;
    }
    public async Task<(bool, bool)> HasVerified(string guardianId)
    {
        var verifications = await _verificationService.GetAllByKeyCeremonyIdAsync(CeremonyDetails.KeyCeremonyId);
        var list = verifications?.Where(v => v.DesignatedId == guardianId);
        var notVerified = list?.Where(v => v.Verified == false);
        return (list?.Count() > 0, notVerified?.Count() > 0);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        _electionPublicKeys?.Dispose();
        _electionPartialKeyBackups?.Dispose();
        _electionPartialKeyChallenges?.Dispose();
    }
}
