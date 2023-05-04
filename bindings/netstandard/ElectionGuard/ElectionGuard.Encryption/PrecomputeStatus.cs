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
        public double Progress;

        /// <summary>
        /// The number of exponentiations that are completed
        /// </summary>
        public long CurrentQueueSize;

        /// <summary>
        /// The total number of exponentiations to be calculated
        /// </summary>
        public long MaxQueueSize;

        /// <summary>
        /// Current status of the precompute process
        /// </summary>
        public PrecomputeState CurrentState;
    }
}
