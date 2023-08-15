using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{

    public static class TestHelpers
    {
        public static async Task RunForAsync(TimeSpan duration)
        {
            var start = DateTime.Now;
            var end = start + duration;
            while (DateTime.Now < end)
            {
                PrintMemory();
                await Task.Delay(1000);
            }
        }

        public static void RunFor(TimeSpan duration)
        {
            var start = DateTime.Now;
            var end = start + duration;
            while (DateTime.Now < end)
            {
                PrintMemory();
                Thread.Sleep(1000);
            }
        }

        public static void PrintMemory()
        {
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            Console.WriteLine($"Memory Size: {workingSet / (1024.0 * 1024.0):F2} MB");
        }
    }
}
