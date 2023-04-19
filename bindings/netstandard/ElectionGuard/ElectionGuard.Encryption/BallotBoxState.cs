namespace ElectionGuard
{
    /// <Summary>
    /// Enumeration used when marking a ballot as cast or spoiled
    /// </Summary>
    public enum BallotBoxState
    {
        /// <summary>
        /// A ballot whose state is unknown to ElectionGuard
        /// and will not be included in any election results
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// A ballot that has been explicitly cast
        /// </summary>
        Cast = 1,
        /// A ballot that never included in the tally BUT included in election record
        /// - contents always decrypted and stored in plain text
        /// - always included in the verification site
        /// - used to be called spoiled
        Challenged = 2,
        /// <summary>
        /// A ballot that never included in the tally
        /// - included in election record for verification
        /// - contents never decrypted
        /// - never included in the verification site
        /// - slightly new meaning of what spoiled is
        /// </summary>
        Spoiled = 3,
        /// <Summary>
        /// A ballot whose state is unknown to ElectionGuard and will not be included in any election results
        /// </Summary>
        Unknown = 999
    }
}
