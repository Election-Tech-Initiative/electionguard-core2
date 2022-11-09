namespace ElectionGuard
{
    /// <summary>
    /// States that the precompute process can be in.
    /// </summary>
    public enum PrecomputeState
    {
        /// <summary>
        /// The precompute has not been started.
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// The precompute is currently running
        /// </summary>
        Running = 1,
        /// <summary>
        /// The user stopped the precompute
        /// </summary>
        UserStopped = 2,
        /// <summary>
        /// The precompute finished
        /// </summary>
        Completed = 3,
    }
}