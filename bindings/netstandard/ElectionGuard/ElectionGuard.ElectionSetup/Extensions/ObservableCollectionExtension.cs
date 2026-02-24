using System.Collections.ObjectModel;

namespace ElectionGuard.ElectionSetup.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
            {
                collection.Add(item);
            }
        }
    }
}
