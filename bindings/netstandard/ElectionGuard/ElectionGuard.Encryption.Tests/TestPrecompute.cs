using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [NonParallelizable]
    [TestFixture]
    //[Category("MemoryLeak")]
    public class TestPrecompute
    {
        private const int MaxCompleteDelay = 30000;
        private const int TestBufferSize = 20000;

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
            Console.WriteLine($"Test_Precompute_Status_Running");

            _precompute.StartPrecomputeAsync();
            var runningStatus = _precompute.GetStatus();

            await RunForAsync(TimeSpan.FromSeconds(TestRunLengthInS));
            //await Task.Delay(1000);

            _precompute.StopPrecompute();

            var status = _precompute.GetStatus();

            Console.WriteLine($"Running: {status.CurrentQueueSize}");

            // Assert.That(status.CurrentQueueSize, Is.GreaterThan(0));
            // Assert.AreEqual(PrecomputeState.Running, runningStatus.CurrentState);
        }

        [Test, Order(4)]
        public async Task Test_Precompute_Status_Complete()
        {
            _precompute.StartPrecomputeAsync();

            await RunForAsync(TimeSpan.FromSeconds(10));

            var waitReturn = _waitHandle.WaitOne();

            var status = _precompute.GetStatus();

            // Assert.That(status.CurrentQueueSize, Is.GreaterThanOrEqualTo(TestBufferSize));
            // Assert.That(status.Progress, Is.GreaterThanOrEqualTo(1.0));
            // Assert.AreEqual(PrecomputeState.Completed, status.CurrentState);
            // Assert.AreEqual(true, waitReturn);
        }

        [Test, Order(4)]
        public void Test_Precompute_Using_Old_Interface()
        {
            var compute = new Precompute();
            compute.StartPrecomputeAsync(_precompute.PublicKey, TestBufferSize);
            RunFor(TimeSpan.FromSeconds(TestRunLengthInS));
            compute.StopPrecompute();
        }

        private static async Task RunForAsync(TimeSpan duration)
        {
            var start = DateTime.Now;
            var end = start + duration;
            while (DateTime.Now < end)
            {
                PrintMemory();
                await Task.Delay(1000);
            }
        }

        private static void RunFor(TimeSpan duration)
        {
            var start = DateTime.Now;
            var end = start + duration;
            while (DateTime.Now < end)
            {
                PrintMemory();
                Thread.Sleep(1000);
            }
        }

        private static void PrintMemory()
        {
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            Console.WriteLine($"Memory Size: {workingSet / (1024.0 * 1024.0):F2} MB");
        }
    }
}
