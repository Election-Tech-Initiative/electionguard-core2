namespace ElectionGuard
{
    /// <Summary>
    /// Enumeration used when marking a ballot as cast or spoiled
    /// </Summary>
    public enum BallotBoxState
    {
        /// <Summary>
        /// A ballot that has been explicitly cast
        /// </Summary>
        Cast = 1,
        /// <Summary>
        /// A ballot that has been explicitly spoiled
        /// </Summary>
        Spoiled = 2,
        /// <Summary>
        /// A ballot whose state is unknown to ElectionGuard and will not be included in any election results
        /// </Summary>
        Unknown = 999
    }
}