
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.ElectionSetup.Tests.KeyCeremony;

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
    public static KeyCeremonyRecord GenerateKeyCeremony(
        int numberOfGuardians,
        int quorum,
        string adminId = "default-admin-id",
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
            var guardianId = sequenceOrder.ToString();
            var guardian = new Guardian(
                guardianId,
                sequenceOrder,
                keyCeremony.NumberOfGuardians,
                keyCeremony.Quorum,
                keyCeremony.Id);
            guardians.Add(guardian);
        }
        return guardians;
    }

    public static KeyCeremonyMediatorHarness GenerateMediator(
        KeyCeremonyRecord keyCeremony,
        List<Guardian> guardians)
    {
        var mediator = new KeyCeremonyMediatorHarness(
            "mediator_1",
            guardians[0].GuardianId,
            keyCeremony);
        return mediator;
    }

    public static TestKeyCeremonyData GenerateKeyCeremonyData(
        int numberOfGuardians,
        int quorum, bool runKeyCeremony = true)
    {
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
        // ROUND 1: Public Key Sharing
        // Announce
        foreach (var guardian in data.Guardians)
        {
            data.Mediator.Announce(guardian.ShareKey());
        }

        // Share Public keys
        foreach (var guardian in data.Guardians)
        {
            var announced = data.Mediator
                .ShareAnnounced(guardian.GuardianId);
            announced!
                .ForEach(guardian.SaveGuardianKey);
        }

        // ROUND 2: Partial Key Verification
        // Share
        foreach (var guardian in data.Guardians)
        {
            _ = guardian.GenerateElectionPartialKeyBackups();
            var backups = guardian.ShareElectionPartialKeyBackups();
            data.Mediator.ReceiveBackups(
                backups.Select(b => new GuardianBackups()
                {
                    KeyCeremonyId = data.KeyCeremony.Id,
                    GuardianId = guardian.GuardianId,
                    DesignatedId = b.DesignatedId,
                    Backup = b
                }).ToList()
            );
        }

        // Receive backups
        foreach (var guardian in data.Guardians)
        {
            var backups = data.Mediator.ShareBackups(guardian.GuardianId);
            backups!.ForEach(guardian.SaveElectionPartialKeyBackup);
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
                data.Mediator.ReceiveElectionPartialKeyVerification(verification!);
            });
        }
        // Publish
        var jointKey = data.Mediator.PublishJointKey();
        return jointKey!;
    }
}


