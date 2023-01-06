namespace ElectionGuard.ElectionSetup.Tests.Integration
{
    internal class EndToEndElectionTest
    {
        private const int NumberOfGuardians = 5;
        private const int Quorum = 3;
        private List<Guardian> _guardians = new();
        private KeyCeremonyMediator? _mediator;

        [Test]
        public void TestEndToEndElection()
        {
            Step0ConfigureElection();
            Step1KeyCeremony();
        }

        private void Step1KeyCeremony()
        {
            for (ulong i = 0; i < NumberOfGuardians; i++)
            {
                var guardianId = (i + 1).ToString();
                var guardian = Guardian.FromNonce(guardianId, i + 1, NumberOfGuardians, Quorum, "testkey");
                _guardians.Add(guardian);
            }

            // Setup mediator
            _mediator = new KeyCeremonyMediator(
                "mediator_1", _guardians[0].CeremonyDetails
            );

            // ROUND 1: Public Key Sharing
            // Announce
            foreach (var guardian in _guardians)
            {
                _mediator.Announce(guardian.ShareKey());
            }

        }

        private void Step0ConfigureElection()
        {
            // var manifest = ElectionFactory().get_simple_manifest_from_file()
        }
    }
}
