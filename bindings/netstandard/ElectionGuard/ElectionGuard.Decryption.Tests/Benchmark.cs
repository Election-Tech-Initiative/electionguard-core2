using System.Diagnostics;

namespace ElectionGuard.Decryption.Tests;

public class BenchmarkResult
{
    public TimeSpan Elapsed { get; set; }
}

public class Benchmark<T> : BenchmarkResult
{
    public T Result { get; set; } = default!;
}
public static class TestExtensions
{
    public static BenchmarkResult Benchmark<T>(
        this T self, Action executor, string tag = "") where T : class
    {
        var sw = new Stopwatch();
        sw.Start();
        executor();
        sw.Stop();
        Console.WriteLine($"{self.GetType().Name} {tag} Elapsed={sw.Elapsed}");
        return new BenchmarkResult { Elapsed = sw.Elapsed };
    }

    public static Benchmark<U> Benchmark<T, U>(
        this T self, Func<U> executor, string tag = "") where T : class
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = executor();
        sw.Stop();
        Console.WriteLine($"{self.GetType().Name} {tag} Elapsed={sw.Elapsed}");
        return new Benchmark<U> { Elapsed = sw.Elapsed, Result = result };
    }

    public static async Task<BenchmarkResult> BenchmarkAsync<T>(
        this T self, Func<Task> executor, string tag = "") where T : class
    {
        var sw = new Stopwatch();
        sw.Start();
        await executor();
        sw.Stop();
        Console.WriteLine($"{self.GetType().Name} {tag} Elapsed={sw.Elapsed}");
        return new BenchmarkResult { Elapsed = sw.Elapsed };
    }

    public static async Task<Benchmark<U>> BenchmarkAsync<T, U>(
        this T self, Func<Task<U>> executor, string tag = "") where T : class
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await executor();
        sw.Stop();
        Console.WriteLine($"{self.GetType().Name} {tag} Elapsed={sw.Elapsed}");
        return new Benchmark<U> { Elapsed = sw.Elapsed, Result = result };
    }
}
