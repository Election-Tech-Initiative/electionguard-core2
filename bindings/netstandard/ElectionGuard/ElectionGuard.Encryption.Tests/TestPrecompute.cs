using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [NonParallelizable]
    [TestFixture]
    public class TestPrecompute
    {
        private const int MaxCompleteDelay = 30000;
        private const int TestBufferSize = 500;
        private const int TestRunLengthInS = 10;

        private PrecomputeBufferContext _precompute;
        private AutoResetEvent _waitHandle;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _waitHandle = new AutoResetEvent(false);

            var keyPair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);
            _precompute = new PrecomputeBufferContext(keyPair.PublicKey, TestBufferSize);
            _precompute.CompletedEvent += status =>
            {
                _ = _waitHandle.Set();
            };
        }

        [Test, Order(1)]
        public void Test_Precompute_Buffer_Size()
        {
            var status = _precompute.GetStatus();
            Assert.AreEqual(TestBufferSize, status.MaxQueueSize);
        }

        [Test, Order(2)]
        public void Test_Precompute_Status_NoStarted()
        {
            var status = _precompute.GetStatus();
            Assert.AreEqual(PrecomputeState.NotStarted, status.CurrentState);
        }

        [Test, Order(3)]
        public async Task Test_Precompute_Status_Running()
        {
            _precompute.StartPrecomputeAsync();
            var runningStatus = _precompute.GetStatus();

            await TestHelpers.RunForAsync(TimeSpan.FromSeconds(1));

            _precompute.StopPrecompute();

            var status = _precompute.GetStatus();

            Console.WriteLine($"Running: {status.CurrentQueueSize}");

            Assert.That(status.CurrentQueueSize, Is.GreaterThan(0));
            Assert.AreEqual(PrecomputeState.Running, runningStatus.CurrentState);
        }

        [Test, Order(4)]
        public async Task Test_Precompute_Status_Complete()
        {
            _precompute.StartPrecomputeAsync();

            await TestHelpers.RunForAsync(TimeSpan.FromSeconds(TestRunLengthInS));

            var waitReturn = _waitHandle.WaitOne();

            var status = _precompute.GetStatus();

            Assert.That(status.CurrentQueueSize, Is.GreaterThanOrEqualTo(TestBufferSize));
            Assert.That(status.Progress, Is.GreaterThanOrEqualTo(1.0));
            Assert.AreEqual(PrecomputeState.Completed, status.CurrentState);
            Assert.AreEqual(true, waitReturn);
        }

        [Test, Order(5)]
        public void Test_Precompute_Using_Old_Interface()
        {
            var compute = new Precompute();
            compute.StartPrecomputeAsync(_precompute.PublicKey, TestBufferSize);
            TestHelpers.RunFor(TimeSpan.FromSeconds(TestRunLengthInS));
            compute.StopPrecompute();
        }
    }
}
