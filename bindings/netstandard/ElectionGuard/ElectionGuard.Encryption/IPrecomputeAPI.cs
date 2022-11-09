using System;
using System.Threading;

namespace ElectionGuard
{
    /// <summary>
    /// Interface for the precompute API to start and stop the precompute calculations
    /// </summary>
    public interface IPrecomputeAPI
    {
        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="maxexp">The max exponentiation to be calculated</param>
        /// <param name="token">CancelationToken that can be used to start the process</param>
        [Obsolete]
        void StartPrecomputeAsync(long maxexp, CancellationToken token);

        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="publicKey">The max exponentiation to be calculated</param>
        /// <param name="buffers">The maximum number of buffers to precompute</param>
        void StartPrecomputeAsync(ElementModP publicKey, int buffers);

        /// <summary>
        /// Stops the precompute process
        /// </summary>
        void StopPrecompute();

        /// <summary>
        /// Get the current status for the current running process
        /// </summary>
        /// <returns><see cref="PrecomputeStatus">PrecomputeStatus</see> with all of the latest information</returns>
        PrecomputeStatus GetStatus();

        /// <summary>
        /// Event handler that will give back progress to the calling code
        /// </summary>
        event StatusEventHandler ProgressEvent;

        /// <summary>
        /// Event handler that will signal that the entire precompute is completed
        /// </summary>
        event StatusEventHandler CompletedEvent;
    }
}