using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup.Tests.Integration;

// A harness for exposing protected methods for testing
public class KeyCeremonyMediatorHarness : KeyCeremonyMediator
{
    public KeyCeremonyMediatorHarness(
        string mediatorId,
        string userId,
        UI.Lib.Models.KeyCeremonyRecord keyCeremony)
    : base(mediatorId, userId, keyCeremony)
    {
    }

    public new List<ElectionPublicKey>? ShareAnnounced(string? requestingGuardianId)
    {
        return base.ShareAnnounced(requestingGuardianId);
    }
}
[TestFixture]
public class EndToEndElectionTest : DisposableBase
{
    private const int NumberOfGuardians = 5;
    private const int Quorum = 3;

    private TestElectionData Data = default!;

    private List<Guardian> _guardians = new();
    private KeyCeremonyMediatorHarness? _mediator;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {

        Data = ElectionGenerator.GenerateFakeElectionData();

    }

    [Test]
    public void TestEndToEndElection()
    {
        Step0ConfigureElection();
        Step1KeyCeremony();
    }

    private void Step0ConfigureElection()
    {
        Assert.That(Data.Manifest.IsValid());

        // TODO: election builder
    }

    private void Step1KeyCeremony()
    {
        // setup a key ceremony
        var keyCeremony = new UI.Lib.Models.KeyCeremonyRecord("testkey", NumberOfGuardians, Quorum, "adminid");

        // Setup guardians
        for (ulong i = 1; i <= NumberOfGuardians; i++)
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
            _mediator.Announce(guardian.ShareKey());
        }

        // Share Public keys
        foreach (var guardian in _guardians)
        {
            var announced = _mediator.ShareAnnounced(guardian.GuardianId);
            announced!//.Where(a => a.OwnerId != guardian.GuardianId).ToList()
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

