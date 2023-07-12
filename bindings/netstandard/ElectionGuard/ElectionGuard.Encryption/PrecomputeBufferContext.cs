using System;
using System.Threading;

namespace ElectionGuard
{
    /// <summary>
    /// Class that controls the precompute process
    /// </summary>
    [Obsolete]
    public class Precompute
    {
        static readonly int DummyBufferSize = 100;   // set a buffer size that will be > 0 and < the default of 5000 for initialization

        PrecomputeBufferContext _context;

        /// <summary>
        /// Default constructor to initialize buffers for testing
        /// </summary>
        public Precompute()
        {
        }


        private PrecomputeStatus _currentStatus = new PrecomputeStatus
        {
            Progress = 0,
            CurrentQueueSize = 0,
            CurrentState = PrecomputeState.NotStarted
        };
        private int _maxBuffers = DummyBufferSize;

        /// <summary>
        /// Event handler that will give back progress to the calling code
        /// </summary>
        public event StatusEventHandler ProgressEvent;

        /// <summary>
        /// Event handler that will signal that the entire precompute is completed
        /// </summary>
        public event StatusEventHandler CompletedEvent;

        /// <summary>
        /// Internal method used to call the completed event
        /// </summary>
        private void OnCompletedEvent()
        {
            CompletedEvent?.Invoke(_currentStatus);
        }

        /// <summary>
        /// Internal method used to call the progress event
        /// </summary>
        private void OnProgressEvent()
        {
            ProgressEvent?.Invoke(_currentStatus);
        }

        /// <summary>
        /// Get the current status for the current running process
        /// </summary>
        /// <returns><see cref="PrecomputeStatus">PrecomputeStatus</see> with all of the latest information</returns>
        public PrecomputeStatus GetStatus()
        {
            return _context.GetStatus();
        }

        /// <summary>
        /// Gets the progress of the precompute
        /// </summary>
        /// <param name="count">count of the buffer entries</param>
        /// <param name="queueSize">max size of the buffer queue</param>
        public void GetProgress(out int count, out int queueSize)
        {
            var status = _context.GetStatus();
            count = (int)status.CurrentQueueSize;
            queueSize = (int)status.MaxQueueSize;
        }

        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="publicKey">The max exponentiation to be calculated</param>
        /// <param name="buffers">The maximum number of buffers to precompute</param>
        public void StartPrecomputeAsync(ElementModP publicKey, int buffers = 0)
        {
            _currentStatus.CurrentState = PrecomputeState.Running;
            _context = new PrecomputeBufferContext(publicKey, buffers);
            _context.StartPrecomputeAsync();
        }

        /// <summary>
        /// Stops the precompute process
        /// </summary>
        public void StopPrecompute()
        {
            _context.StopPrecompute();
            SendCompleted();
        }


        private void SendCompleted()
        {
            int count = -1;
            int queueSize = -2;
            GetProgress(out count, out queueSize);
            if (count == queueSize)
                _currentStatus.CurrentState = PrecomputeState.Completed;
            _currentStatus.Progress = (double)count / queueSize;
            _currentStatus.CurrentQueueSize = count;

            OnCompletedEvent();
        }

    }





    /// <summary>
    /// Delegate definition to return back the status of the precompute process
    /// </summary>
    /// <returns><see cref="PrecomputeStatus">PrecomputeStatus</see> with all of the latest information</returns>
    public delegate void StatusEventHandler(PrecomputeStatus status);

    /// <summary>
    /// A singleton context for a collection of precomputed triples and quadruples.
    /// </summary>
    public class PrecomputeBufferContext : DisposableBase
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

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="publicKey">Key to use</param>
        /// <param name="maxQueueSize">Size of the queue</param>
        public PrecomputeBufferContext(
            ElementModP publicKey, int maxQueueSize = 5000)
        {
            _elgamalPublicKey = new ElementModP(publicKey);
            var status = NativeInterface.PrecomputeBufferContext.Initialize(
                _elgamalPublicKey.Handle, maxQueueSize);
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

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            _elgamalPublicKey.Dispose();
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
            _ = _waitHandle.Set();
            _ = NativeInterface.PrecomputeBufferContext.Start();

            ReportStatus();
        }

        private void WorkerMethod()
        {
            _ = _waitHandle.Set();
            _ = NativeInterface.PrecomputeBufferContext.Start(_elgamalPublicKey.Handle);

            ReportStatus();
        }
    }
}
