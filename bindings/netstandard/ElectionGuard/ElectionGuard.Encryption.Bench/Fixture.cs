using System;
using System.Diagnostics;

namespace ElectionGuard.Encryption.Bench
{
    public abstract class Fixture
    {
        public virtual void SetUp()
        {

        }

        public virtual void TearDown()
        {

        }

        public abstract void Run();

        public virtual void Run(Action executor)
        {
            SetUp();
            BenchmarkCase(executor);
            TearDown();
        }

        protected virtual void BenchmarkCase(Action executor)
        {
            var sw = new Stopwatch();
            sw.Start();
            executor();
            sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.Elapsed);
        }
    }
}
