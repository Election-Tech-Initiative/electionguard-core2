using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.ElectionSetup.Tests.KeyCeremony;

// A harness for exposing protected methods for testing
public class KeyCeremonyMediatorHarness : KeyCeremonyMediator
{
    public IKeyCeremonyService Service => _keyCeremonyService;
    public IGuardianBackupService BackupService => _backupService;
    public IGuardianPublicKeyService PublicKeyService => _publicKeyService;
    public IVerificationService VerificationService => _verificationService;

    public KeyCeremonyMediatorHarness(
        string mediatorId,
        string userId,
        KeyCeremonyRecord keyCeremony,
        IKeyCeremonyService keyCeremonyService,
        IGuardianBackupService backupService,
        IGuardianPublicKeyService publicKeyService,
        IVerificationService verificationService)
    : base(
        mediatorId,
        userId,
        keyCeremony,
        keyCeremonyService,
        backupService,
        publicKeyService,
        verificationService)
    {
    }

    public new List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        return base.ShareAnnounced(requestingGuardianId);
    }
}

