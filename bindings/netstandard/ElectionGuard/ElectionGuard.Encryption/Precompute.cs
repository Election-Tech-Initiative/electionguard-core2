using System;
using System.Threading;
// ReSharper disable RedundantAssignment
// ReSharper disable InlineOutVariableDeclaration

namespace ElectionGuard
{
    /// <summary>
    /// Class that controls the precompute process
    /// </summary>
    public class Precompute : IPrecomputeApi
    {
        static readonly int DummyBufferSize = 100;   // set a buffer size that will be > 0 and < the default of 5000 for initialization

        readonly int _initCount = -1;            // initializer for count to make sure we are getting a value back from the C++
        readonly int _initQueueSize = -2;       // initializer for the queue size to make sure that its different than the count and being set

        AutoResetEvent _waitHandle;

        /// <summary>
        /// Default constructor to initialize buffers for testing
        /// </summary>
        public Precompute()
        {
            NativeInterface.PrecomputeBuffers.Init(_maxBuffers);
        }


        private PrecomputeStatus _currentStatus = new PrecomputeStatus
        {
            Percentage = 0,
            CompletedExponentiationsCount = 0,
            CurrentState = PrecomputeState.NotStarted
        };
        private Thread _workerThread;
        private int _maxBuffers = DummyBufferSize;
        private ElementModP _elgamalPublicKey;

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
            int count = _initCount;
            int queueSize = _initQueueSize;
            GetProgress(out count, out queueSize);
            if (count == queueSize)
                _currentStatus.CurrentState = PrecomputeState.Completed;
            _currentStatus.Percentage = (double)count / queueSize;
            _currentStatus.CompletedExponentiationsCount = count;

            return _currentStatus;
        }

        /// <summary>
        /// Gets the progress of the precompute
        /// </summary>
        /// <param name="count">count of the buffer entries</param>
        /// <param name="queueSize">max size of the buffer queue</param>
        public void GetProgress(out int count, out int queueSize)
        {
            NativeInterface.PrecomputeBuffers.Status(out count, out queueSize);
        }

        /// <summary>
        /// Starts the precompute process by creating a new thread to run the process
        /// </summary>
        /// <param name="publicKey">The max exponentiation to be calculated</param>
        /// <param name="buffers">The maximum number of buffers to precompute</param>
        public void StartPrecomputeAsync(ElementModP publicKey, int buffers = 0)
        {
            _currentStatus.CurrentState = PrecomputeState.Running;
            _maxBuffers = buffers;
            _elgamalPublicKey = publicKey;

            _waitHandle = new AutoResetEvent(false);
            _workerThread = new Thread(WorkerMethod)
            {
                Name = "Precompute Worker Thread"
            };
            _workerThread.Start();

            _waitHandle.WaitOne();   // make sure thread is created before returning
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
            else
            {
                // if this is already stopped or completed, resend the Completed event to make sure calling app sees the stopping
                SendCompleted();
            }
            NativeInterface.PrecomputeBuffers.Stop();     // tell the calculations to 
        }

        /// <summary>
        /// Dummy method to be used for the stubbing out until the exponentiation code is complete
        /// </summary>
        private void WorkerMethod()
        {
            NativeInterface.PrecomputeBuffers.Init(_maxBuffers);
            _waitHandle.Set();
            NativeInterface.PrecomputeBuffers.Populate(_elgamalPublicKey.Handle);

            SendCompleted();
        }

        private void SendCompleted()
        {
            int count = -1;
            int queueSize = -2;
            GetProgress(out count, out queueSize);
            if (count == queueSize)
                _currentStatus.CurrentState = PrecomputeState.Completed;
            _currentStatus.Percentage = (double)count / queueSize;
            _currentStatus.CompletedExponentiationsCount = count;

            OnCompletedEvent();
        }

    }
}