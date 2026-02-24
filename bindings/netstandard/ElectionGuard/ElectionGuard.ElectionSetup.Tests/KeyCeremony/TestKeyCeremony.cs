using ElectionGuard.ElectionSetup.Tests.Generators;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.ElectionSetup.Tests.KeyCeremony;
[TestFixture]
public class TestKeyCeremony : DisposableBase
{

    [Test]
    public void Test_KeyCeremony()
    {
        var numberOfGuardians = 5;
        var quorum = 3;
        List<Guardian> guardians = new();
        KeyCeremonyMediatorHarness? mediator;

        // setup a key ceremony
        var keyCeremony = KeyCeremonyGenerator.GenerateKeyCeremony(
            numberOfGuardians, quorum);

        // Setup guardians
        for (ulong sequenceOrder = 1; sequenceOrder <= (ulong)numberOfGuardians; sequenceOrder++)
        {
            var guardianId = sequenceOrder.ToString();
            var random = new Random((int)sequenceOrder);
            var guardian = new Guardian(
                guardianId, sequenceOrder, numberOfGuardians,
                quorum, keyCeremony.Id,
                random);
            guardians.Add(guardian);
        }

        // Setup mediator
        mediator = KeyCeremonyGenerator.GenerateMediator(keyCeremony, guardians);

        // ROUND 1: Public Key Sharing
        // Announce
        foreach (var guardian in guardians)
        {
            mediator.Announce(guardian.SharePublicKey());
        }

        // Share Public keys
        foreach (var guardian in guardians)
        {
            var announced = mediator.ShareAnnounced(guardian.GuardianId);
            announced!
                .ForEach(guardian.AddGuardianKey);
        }

        Assert.That(mediator.AllGuardiansAnnounced());

        // ROUND 2: Partial Key Verification
        // Share backups
        foreach (var sender in guardians)
        {
            Assert.That(sender.GenerateElectionPartialKeyBackups());
            var backups = sender.ShareElectionPartialKeyBackups();
            mediator.ReceiveBackups(
                backups.Select(b => new GuardianBackups(
                    keyCeremony.Id,
                    sender.GuardianId,
                    b.DesignatedId!,
                    new(b)
                )).ToList()
            );
        }

        Assert.That(mediator.AllBackupsAvailable());

        // Receive backups
        foreach (var guardian in guardians)
        {
            var backups = mediator.ShareBackups(guardian.GuardianId);
            backups!.ForEach(g => guardian.SaveElectionPartialKeyBackup(g.ToRecord()));
        }

        // ROUND 3: Partial Key Verification
        // Verify backups
        foreach (var guardian in guardians)
        {
            guardians
            .ForEach(owner =>
            {
                var verification = guardian.VerifyElectionPartialKeyBackup(owner.GuardianId, keyCeremony.Id);
                mediator.ReceiveElectionPartialKeyVerification(new(verification!));
            });
        }

        Assert.That(mediator.AllBackupsVerified(), mediator.GetVerificationState().ToString());

        // Publish Joint Key
        var jointKey = mediator.PublishJointKey();
        Assert.That(jointKey, Is.Not.EqualTo(null));
    }

    [Test]
    public async Task Test_KeyCeremony_StateMachine()
    {
        var numberOfGuardians = 5;
        var quorum = 3;
        var keyCeremony = KeyCeremonyGenerator.GenerateKeyCeremony(
            numberOfGuardians,
            quorum);
        var mediators = KeyCeremonyGenerator.GenerateMediators(keyCeremony, numberOfGuardians);

        keyCeremony.Id = keyCeremony.KeyCeremonyId!;
        _ = await mediators.First().Service.SaveAsync(keyCeremony);

        await Assert_ShouldStart_Step1(OnlyGuardians(mediators));
        await RunKeyCeremony(OnlyGuardians(mediators));
        await Assert_ShouldStart_Step2(OnlyAdmin(mediators));
        await RunKeyCeremony(OnlyAdmin(mediators));
        await Assert_ShouldStart_Step3(OnlyGuardians(mediators));
        await RunKeyCeremony(OnlyGuardians(mediators));
        await Assert_ShouldStart_Step4(OnlyAdmin(mediators));
        await RunKeyCeremony(OnlyAdmin(mediators));
        await Assert_ShouldStart_Step5(OnlyGuardians(mediators));
        await RunKeyCeremony(OnlyGuardians(mediators));
        await Assert_ShouldStart_Step6(OnlyAdmin(mediators));
        await RunKeyCeremony(OnlyAdmin(mediators));

        Console.WriteLine("Key Ceremony Complete");

    }

    private static List<KeyCeremonyMediatorHarness> OnlyGuardians(List<KeyCeremonyMediatorHarness> mediators)
        => mediators.Where(i => i.UserId != KeyCeremonyGenerator.DEFAULT_ADMIN_ID).ToList();

    private static List<KeyCeremonyMediatorHarness> OnlyAdmin(List<KeyCeremonyMediatorHarness> mediators)
        => mediators.Where(i => i.UserId == KeyCeremonyGenerator.DEFAULT_ADMIN_ID).ToList();

    private static async Task RunKeyCeremony(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            await mediator.RunKeyCeremony(
                isAdmin: mediator.UserId == KeyCeremonyGenerator.DEFAULT_ADMIN_ID);
        }
    }

    private async Task Assert_ShouldStart_Step1(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldGuardianRunStep1());
        }
    }

    private async Task Assert_ShouldStart_Step2(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldAdminStartStep2());
        }
    }

    private async Task Assert_ShouldStart_Step3(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldGuardianRunStep3(), $"mediator {mediator.UserId}");
        }
    }

    private async Task Assert_ShouldStart_Step4(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldAdminStartStep4());
        }
    }

    private async Task Assert_ShouldStart_Step5(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldGuardianRunStep5());
        }
    }

    private async Task Assert_ShouldStart_Step6(List<KeyCeremonyMediatorHarness> mediators)
    {
        foreach (var mediator in mediators)
        {
            Assert.That(await mediator.ShouldAdminStartStep6(), "mediator " + mediator.UserId);
        }
    }


}
