using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.ElectionSetup.Extensions;

public static class DictionaryExtensions
{
    public static void Dispose<T>(this Dictionary<string, T> source) where T : IDisposable
    {
        if (source is null)
        {
            return;
        }
        foreach (var item in source.Values)
        {
            item.Dispose();
        }
        source.Clear();
    }

    public static void Dispose<T>(this Dictionary<GuardianPair, T> source) where T : IDisposable
    {
        if (source is null)
        {
            return;
        }
        foreach (var item in source.Values)
        {
            item.Dispose();
        }
        source.Clear();
    }
}
