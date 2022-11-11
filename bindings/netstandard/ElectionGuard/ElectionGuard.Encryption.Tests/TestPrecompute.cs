using System.Threading;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    class TestPrecompute
    {
        private const int MaxCompleteDelay = 7000;
        private const int SmallBufferSize = 10;
        private const int DefaultBufferSize = 5000;
        private const int LargeBufferSize = 6000;

        [Test]
        public void Test_Precompute_Status_NoStarted()
        {
            Precompute precompute = new Precompute();

            var status = precompute.GetStatus();

            Assert.AreEqual(PrecomputeState.NotStarted, status.CurrentState);
        }

        [Test]
        public void Test_Precompute_Status_Running()
        {
            var waitHandle = new AutoResetEvent(false);

            Precompute precompute = new Precompute();
            var keypair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);

            precompute.CompletedEvent += completedStatus =>
            {
                Assert.AreEqual(PrecomputeState.UserStopped, completedStatus.CurrentState);
                waitHandle.Set();
            };
            precompute.StartPrecomputeAsync(keypair.PublicKey, LargeBufferSize);
            var runningStatus = precompute.GetStatus();
            precompute.StopPrecompute();

            var waitReturn = waitHandle.WaitOne(MaxCompleteDelay);

            precompute.GetProgress(out _, out var queueSize);
            var status = precompute.GetStatus();

            Assert.AreEqual(LargeBufferSize, queueSize);
            Assert.AreEqual(PrecomputeState.Running, runningStatus.CurrentState);
            Assert.AreEqual(true, waitReturn);
            Assert.AreEqual(PrecomputeState.UserStopped, status.CurrentState);
        }

        [Test]
        public void Test_Precompute_Status_Stopped()
        {
            var waitHandle = new AutoResetEvent(false);

            Precompute precompute = new Precompute();
            var keypair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);

            precompute.CompletedEvent += completedStatus =>
            {
                Assert.AreEqual(PrecomputeState.UserStopped, completedStatus.CurrentState);
                waitHandle.Set();
            };
            precompute.StartPrecomputeAsync(keypair.PublicKey);
            var statusRunning = precompute.GetStatus();
            precompute.StopPrecompute();

            var waitReturn = waitHandle.WaitOne(MaxCompleteDelay);

            // ReSharper disable once InlineOutVariableDeclaration
            // ReSharper disable once RedundantAssignment
            var count = -1;
            // ReSharper disable once InlineOutVariableDeclaration
            // ReSharper disable once RedundantAssignment
            var queueSize = -1;
            precompute.GetProgress(out count, out queueSize);
            var status = precompute.GetStatus();

            Assert.AreEqual(DefaultBufferSize, queueSize);
            Assert.AreNotEqual(-1, count);

            Assert.AreEqual(PrecomputeState.Running, statusRunning.CurrentState);
            Assert.AreEqual(PrecomputeState.UserStopped, status.CurrentState);

            Assert.AreEqual(true, waitReturn);
        }

        [Test]
        public void Test_Precompute_Status_Complete()
        {
            var waitHandle = new AutoResetEvent(false);

            Precompute precompute = new Precompute();
            var keypair = ElGamalKeyPair.FromSecret(Constants.TWO_MOD_Q);

            precompute.CompletedEvent += completedStatus =>
            {
                Assert.AreEqual(SmallBufferSize, completedStatus.CompletedExponentiationsCount);
                Assert.AreEqual(1.0, completedStatus.Percentage);
                Assert.AreEqual(PrecomputeState.Completed, completedStatus.CurrentState);
                waitHandle.Set();
            };
            precompute.StartPrecomputeAsync(keypair.PublicKey, SmallBufferSize);

            var waitReturn = waitHandle.WaitOne(MaxCompleteDelay);

            precompute.GetProgress(out var count, out var queueSize);
            var status = precompute.GetStatus();

            Assert.AreEqual(SmallBufferSize, queueSize);
            Assert.AreEqual(SmallBufferSize, count);

            Assert.AreEqual(PrecomputeState.Completed, status.CurrentState);
            Assert.AreEqual(true, waitReturn);
        }

    }
}
