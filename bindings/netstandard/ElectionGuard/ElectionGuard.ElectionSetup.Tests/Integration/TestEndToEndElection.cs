using ElectionGuard.Encryption.Utils.Generators;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.ElectionSetup.Tests.Generators;

namespace ElectionGuard.ElectionSetup.Tests.Integration;
[TestFixture]
public class TestEndToEndElection : DisposableBase
{
    private const int NumberOfGuardians = 5;
    private const int Quorum = 3;

    private TestElectionData Data = default!;
    private TestKeyCeremonyData KeyCeremonyData = default!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Data = ElectionGenerator.GenerateFakeElectionData();
        KeyCeremonyData = KeyCeremonyGenerator.GenerateKeyCeremonyData(
            NumberOfGuardians,
            Quorum, runKeyCeremony: false);
    }

    [Test]
    public void Test_EndToEnd_Election()
    {
        Step0_ConfigureElection();
        Step1_KeyCeremony();
    }

    private void Step0_ConfigureElection()
    {
        Assert.That(Data.Manifest.IsValid());

        // TODO: election builder
    }

    private void Step1_KeyCeremony()
    {
        KeyCeremonyData.JointKey = KeyCeremonyGenerator.RunKeyCeremony(
            KeyCeremonyData);

        Assert.That(KeyCeremonyData.JointKey is not null);
    }
}

