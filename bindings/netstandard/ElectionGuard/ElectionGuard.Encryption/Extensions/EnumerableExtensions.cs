using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectionGuard.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source, Func<T, Task> action)
        {
            if (source.IsNullOrEmpty())
            {
                return;
            }

            foreach (var item in source)
            {
                await action(item);
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
        {
            return self == null || !self.Any();
        }

        public static IList<T> Shuffle<T>(this IList<T> source, Random random = null)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (random == null)
            {
                random = new Random(unchecked(Environment.TickCount * 31));
            }

            var next = source.Count;
            while (next > 1)
            {
                next--;
                var k = random.Next(next + 1);
                (source[next], source[k]) = (source[k], source[next]);
            }
            return source;
        }

        public static IEnumerable<(T item, ulong index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, (ulong)index));
        }

        public static void Dispose(this IEnumerable<DisposableBase> source)
        {
            if (source.IsNullOrEmpty())
            {
                return;
            }

            foreach (var item in source)
            {
                item.Dispose();
            }
        }
    }
}
