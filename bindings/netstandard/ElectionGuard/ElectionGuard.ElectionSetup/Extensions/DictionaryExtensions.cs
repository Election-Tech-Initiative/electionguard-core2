namespace ElectionGuard.ElectionSetup.Extensions;

public static class DictionaryExtensions
{
    public static void Dispose<T>(this Dictionary<string, T> source)
    {
        foreach (var item in source.Values)
        {
            (item as DisposableBase)?.Dispose();
        }
        source.Clear();
    }

    public static void Dispose<T>(this Dictionary<GuardianPair, T> source)
    {
        foreach (var item in source.Values)
        {
            (item as DisposableBase)?.Dispose();
        }
        source.Clear();
    }
}
