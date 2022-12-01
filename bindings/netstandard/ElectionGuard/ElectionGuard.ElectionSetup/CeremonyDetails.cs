namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Details of key ceremony
    /// </summary>
    public class CeremonyDetails
    {
        public CeremonyDetails(int numberOfGuardians, int quorum)
        {
            NumberOfGuardians = numberOfGuardians;
            Quorum = quorum;
        }

        private int NumberOfGuardians { get; }
        private int Quorum { get; }
    }
}