
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.ElectionSetup.Tests.KeyCeremony;
using ElectionGuard.ElectionSetup.Tests.Mocks;

namespace ElectionGuard.ElectionSetup.Tests.Generators;

public class TestKeyCeremonyData : DisposableBase
{
    public int NumberOfGuardians { get; init; } = 5;
    public int Quorum { get; init; } = 3;
    public KeyCeremonyRecord KeyCeremony { get; init; }
    public List<Guardian> Guardians { get; init; }
    public KeyCeremonyMediatorHarness Mediator { get; init; }
    public ElectionJointKey? JointKey { get; set; }

    public TestKeyCeremonyData(
        int numberOfGuardians,
        int quorum,
        KeyCeremonyRecord keyCeremony,
        List<Guardian> guardians,
        KeyCeremonyMediatorHarness mediator)
    {
        NumberOfGuardians = numberOfGuardians;
        Quorum = quorum;
        KeyCeremony = keyCeremony;
        Guardians = guardians;
        Mediator = mediator;
    }
}

public class KeyCeremonyGenerator
{
    public const string DEFAULT_ADMIN_ID = "default-admin-id";
    public static KeyCeremonyRecord GenerateKeyCeremony(
        int numberOfGuardians,
        int quorum,
        string adminId = DEFAULT_ADMIN_ID,
        string name = "test-key-ceremony")
    {
        var keyCeremony = new KeyCeremonyRecord(
            name,
            numberOfGuardians, quorum, adminId);
        return keyCeremony;
    }

    public static List<Guardian> GenerateGuardians(
        KeyCeremonyRecord keyCeremony)
    {

        var guardians = new List<Guardian>();
        for (ulong sequenceOrder = 1;
            sequenceOrder <= (ulong)keyCeremony.NumberOfGuardians;
            sequenceOrder++)
        {
            var random = new Random((int)sequenceOrder);
            var guardianId = sequenceOrder.ToString();
            var guardian = new Guardian(
                $"guardian_{guardianId}",
                sequenceOrder,
                keyCeremony.NumberOfGuardians,
                keyCeremony.Quorum,
                keyCeremony.Id,
                random);
            guardians.Add(guardian);
        }
        return guardians;
    }

    // generate a single mediator for the key ceremony
    public static KeyCeremonyMediatorHarness GenerateMediator(
        KeyCeremonyRecord keyCeremony,
        List<Guardian> guardians)
    {
        var service = new MockKeyCeremonyService();
        var publicKeys = new MockGuardianPublicKeyService();
        var verification = new MockVerificationService();
        var backups = new MockGuardianBackupService();
        var mediator = new KeyCeremonyMediatorHarness(
            "mediator_guardian_1",
            guardians[0].GuardianId,
            keyCeremony,
            service,
            backups,
            publicKeys,
            verification);
        return mediator;
    }

    // generate individual mediators for each admin and guardian
    public static List<KeyCeremonyMediatorHarness> GenerateMediators(
        KeyCeremonyRecord keyCeremony,
        int numberOfGuardians)
    {
        var service = new MockKeyCeremonyService();
        var publicKeys = new MockGuardianPublicKeyService();
        var verification = new MockVerificationService();
        var backups = new MockGuardianBackupService();
        List<KeyCeremonyMediatorHarness> mediators = new();
        var adminMediator = new KeyCeremonyMediatorHarness(
            "mediator_admin",
            DEFAULT_ADMIN_ID,
            keyCeremony,
            service,
            backups,
            publicKeys,
            verification);
        mediators.Add(adminMediator);
        foreach (var i in Enumerable.Range(1, numberOfGuardians))
        {
            Console.WriteLine($"Generating mediator for guardian {i}...");
            var mediator = new KeyCeremonyMediatorHarness(
                $"mediator_guardian_{i}",
                $"guardian_{i}",
                keyCeremony,
                service,
                backups,
                publicKeys,
                verification);
            mediators.Add(mediator);
        }
        return mediators;
    }

    public static TestKeyCeremonyData GenerateKeyCeremonyData(
        int numberOfGuardians,
        int quorum, bool runKeyCeremony = true)
    {
        Console.WriteLine($"Generating Key Ceremony Data for {numberOfGuardians} guardians and quorum {quorum}...");
        var keyCeremony = GenerateKeyCeremony(
            numberOfGuardians,
            quorum);
        var guardians = GenerateGuardians(keyCeremony);
        var mediator = GenerateMediator(keyCeremony, guardians);
        var result = new TestKeyCeremonyData(
            numberOfGuardians,
            quorum,
            keyCeremony,
            guardians,
            mediator);

        if (runKeyCeremony)
        {
            result.JointKey = RunKeyCeremony(result);
        }

        return result;
    }

    public static ElectionJointKey RunKeyCeremony(
       TestKeyCeremonyData data)
    {
        Console.WriteLine("Running Key Ceremony...");

        // ROUND 1: Public Key Sharing
        // Announce
        foreach (var guardian in data.Guardians)
        {
            data.Mediator.Announce(guardian.SharePublicKey());
        }

        // Share Public keys
        foreach (var guardian in data.Guardians)
        {
            var announced = data.Mediator
                .ShareAnnounced(guardian.GuardianId);
            announced!
                .ForEach(guardian.AddGuardianKey);
        }

        // ROUND 2: Partial Key Verification
        // Share
        foreach (var guardian in data.Guardians)
        {
            _ = guardian.GenerateElectionPartialKeyBackups();
            var backups = guardian.ShareElectionPartialKeyBackups();
            data.Mediator.ReceiveBackups(
                backups.Select(b => new GuardianBackups(
                    data.KeyCeremony.Id,
                    guardian.GuardianId,
                    b.DesignatedId!,
                    new(b))
            ).ToList());
        }

        // Receive backups
        foreach (var guardian in data.Guardians)
        {
            var backups = data.Mediator.ShareBackups(guardian.GuardianId);
            backups!.ForEach(g=>guardian.SaveElectionPartialKeyBackup(g.ToRecord()));
        }

        // ROUND 3: Joint Key
        // Verify backups
        foreach (var guardian in data.Guardians)
        {
            data.Guardians
            .ForEach(owner =>
            {
                var verification = guardian.VerifyElectionPartialKeyBackup(
                    owner.GuardianId, data.KeyCeremony.Id);
                data.Mediator.ReceiveElectionPartialKeyVerification(new(verification!));
            });
        }
        Assert.That(data.Mediator.AllBackupsVerified(), data.Mediator.GetVerificationState().ToString());

        // Publish
        var jointKey = data.Mediator.PublishJointKey();
        return jointKey!;
    }
}


