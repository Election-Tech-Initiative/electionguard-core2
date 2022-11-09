namespace ElectionGuard
{
    /// <summary>
    /// Status for the precompute process
    /// </summary>
    public struct PrecomputeStatus
    {
        /// <summary>
        /// Percentage of the calculations that are complete
        /// </summary>
        public double Percentage;

        /// <summary>
        /// The number of exponentiations that are completed
        /// </summary>
        public long CompletedExponentiationsCount;

        /// <summary>
        /// Current status of the precompute process
        /// </summary>
        public PrecomputeState CurrentState;
    }
}
