namespace ElectionGuard.ElectionSetup.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<(T item, ulong index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, (ulong)index));
    }

    public static void Dispose(this IEnumerable<DisposableBase> source)
    {
        if (source == null) return;
        foreach (var item in source)
        {
            item.Dispose();
        }
    }
}
