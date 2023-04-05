using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup.Tests.KeyCeremony;
[TestFixture]
public class TestKeyCeremony : DisposableBase
{

    [Test]
    public void Test_KeyCeremony()
    {
        var NumberOfGuardians = 5;
        var Quorum = 3;
        List<Guardian> _guardians = new();
        KeyCeremonyMediatorHarness? _mediator;

        // setup a key ceremony
        var keyCeremony = new KeyCeremonyRecord("testkey", NumberOfGuardians, Quorum, "adminid");

        // Setup guardians
        for (ulong i = 1; i <= (ulong)NumberOfGuardians; i++)
        {
            var guardianId = i.ToString();
            var guardian = new Guardian(guardianId, i, NumberOfGuardians, Quorum, keyCeremony.Id);
            _guardians.Add(guardian);
        }

        // Setup mediator
        _mediator = new KeyCeremonyMediatorHarness("mediator_1", _guardians[0].GuardianId, keyCeremony);

        // ROUND 1: Public Key Sharing
        // Announce
        foreach (var guardian in _guardians)
        {
            _mediator.Announce(guardian.SharePublicKey());
        }

        // Share Public keys
        foreach (var guardian in _guardians)
        {
            var announced = _mediator.ShareAnnounced(guardian.GuardianId);
            announced!
                .ForEach(guardian.SaveGuardianKey);
        }

        Assert.That(_mediator.AllGuardiansAnnounced());

        // ROUND 2: Partial Key Verification
        // Share backups
        foreach (var sender in _guardians)
        {
            Assert.That(sender.GenerateElectionPartialKeyBackups());
            var backups = sender.ShareElectionPartialKeyBackups();
            _mediator.ReceiveBackups(
                backups.Select(b => new GuardianBackups()
                {
                    KeyCeremonyId = keyCeremony.Id,
                    GuardianId = sender.GuardianId,
                    DesignatedId = b.DesignatedId,
                    Backup = b
                }).ToList()
            );
        }

        Assert.That(_mediator.AllBackupsAvailable());

        // Receive backups
        foreach (var guardian in _guardians)
        {
            var backups = _mediator.ShareBackups(guardian.GuardianId);
            backups!.ForEach(guardian.SaveElectionPartialKeyBackup);
        }

        // ROUND 3: Partial Key Verification
        // Verify backups
        foreach (var guardian in _guardians)
        {
            _guardians
            .ForEach(owner =>
            {
                var verification = guardian.VerifyElectionPartialKeyBackup(owner.GuardianId, keyCeremony.Id);
                _mediator.ReceiveElectionPartialKeyVerification(verification!);
            });
        }

        Assert.That(_mediator.AllBackupsVerified(), _mediator.GetVerificationState().ToString());

        // Publish Joint Key
        var jointKey = _mediator.PublishJointKey();
        Assert.That(jointKey, Is.Not.EqualTo(null));
    }
}
