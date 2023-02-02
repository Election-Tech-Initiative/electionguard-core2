namespace ElectionGuard.ElectionSetup.Extensions;

public static class DictionaryExtensions
{
    public static void Dispose<T>(this Dictionary<string, T> source) where T : DisposableBase
    {
        foreach (var item in source.Values)
        {
            item.Dispose();
        }
        source.Clear();
    }

    public static void Dispose<T>(this Dictionary<GuardianPair, T> source) where T : DisposableBase
    {
        foreach (var item in source.Values)
        {
            item.Dispose();
        }
        source.Clear();
    }
}
