using System.Threading;

namespace ElectionGuard
{
    /// <summary>
    /// Delegate definition to return back the status of the precompute process
    /// </summary>
    /// <returns><see cref="PrecomputeStatus">PrecomputeStatus</see> with all of the latest information</returns>
    public delegate void StatusEventHandler(PrecomputeStatus status);

    /// <summary>
    /// A singleton context for a collection of precomputed triples and quadruples.
    /// </summary>
    public class PrecomputeBufferContext
    {
        private ElementModP _elgamalPublicKey;

        private AutoResetEvent _waitHandle;
        private Thread _workerThread;

        public event StatusEventHandler ProgressEvent;
        public event StatusEventHandler CompletedEvent;

        private PrecomputeStatus _currentStatus = new PrecomputeStatus
        {
            Progress = 0,
            CurrentQueueSize = 0,
            CurrentState = PrecomputeState.NotStarted
        };

        public PrecomputeBufferContext(
            ElementModP publicKey, int maxQueueSize = 5000)
        {
            var status = NativeInterface.PrecomputeBufferContext.Initialize(
                publicKey.Handle, maxQueueSize);
            status.ThrowIfError();
        }

        /// <summary>
        /// Get the current status for the current running process
        /// </summary>
        /// <returns><see cref="PrecomputeStatus">PrecomputeStatus</see> with all of the latest information</returns>
        public PrecomputeStatus GetStatus()
        {
            var status = NativeInterface.PrecomputeBufferContext.Status(
                out var count, out var queueSize);
            status.ThrowIfError();

            if (count >= queueSize)
            {
                _currentStatus.CurrentState = PrecomputeState.Completed;
            }

            _currentStatus.Progress = (double)count / queueSize;
            _currentStatus.CurrentQueueSize = count;
            _currentStatus.MaxQueueSize = queueSize;

            return _currentStatus;
        }

        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="publicKey">The max exponentiation to be calculated</param>
        /// <param name="buffers">The maximum number of buffers to precompute</param>
        public void StartPrecomputeAsync()
        {
            _currentStatus.CurrentState = PrecomputeState.Running;

            // start a background thread to do the work
            _waitHandle = new AutoResetEvent(false);
            _workerThread = new Thread(WorkerMethodNoKey)
            {
                Name = "Precompute Worker Thread"
            };
            _workerThread.Start();

            // make sure thread is created before returning
            _ = _waitHandle.WaitOne();
        }

        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="publicKey">The max exponentiation to be calculated</param>
        /// <param name="buffers">The maximum number of buffers to precompute</param>
        public void StartPrecomputeAsync(ElementModP publicKey)
        {
            _currentStatus.CurrentState = PrecomputeState.Running;
            _elgamalPublicKey = new ElementModP(publicKey);

            // start a background thread to do the work
            _waitHandle = new AutoResetEvent(false);
            _workerThread = new Thread(WorkerMethod)
            {
                Name = "Precompute Worker Thread"
            };
            _workerThread.Start();

            // make sure thread is created before returning
            _ = _waitHandle.WaitOne();
        }

        /// <summary>
        /// Stops the precompute process
        /// </summary>
        public void StopPrecompute()
        {
            if (_currentStatus.CurrentState == PrecomputeState.Running)
            {
                _currentStatus.CurrentState = PrecomputeState.UserStopped;
            }

            // tell the calculations to stop
            var status = NativeInterface.PrecomputeBufferContext.Stop();
            status.ThrowIfError();
            ReportStatus();
        }

        private void ReportStatus()
        {
            var status = GetStatus();

            if (status.CurrentState == PrecomputeState.Completed)
            {
                CompletedEvent?.Invoke(status);
            }
            else if (status.CurrentState == PrecomputeState.Running)
            {
                ProgressEvent?.Invoke(status);
            }
        }

        private void WorkerMethodNoKey()
        {
            // _ = NativeInterface.PrecomputeBuffers.Init(_maxQueueSize);
            _ = _waitHandle.Set();
            _ = NativeInterface.PrecomputeBufferContext.Start();

            ReportStatus();
        }

        private void WorkerMethod()
        {
            // _ = NativeInterface.PrecomputeBuffers.Init(_maxQueueSize);
            _ = _waitHandle.Set();
            _ = NativeInterface.PrecomputeBufferContext.Start(_elgamalPublicKey.Handle);

            ReportStatus();
        }
    }
}
