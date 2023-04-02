using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup.Tests.KeyCeremony;

// A harness for exposing protected methods for testing
public class KeyCeremonyMediatorHarness : KeyCeremonyMediator
{
    public KeyCeremonyMediatorHarness(
        string mediatorId,
        string userId,
        KeyCeremonyRecord keyCeremony)
    : base(mediatorId, userId, keyCeremony)
    {
    }

    public new List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        return base.ShareAnnounced(requestingGuardianId);
    }
}

