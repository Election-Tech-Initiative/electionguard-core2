using System;

namespace ElectionGuard
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates a random ElementModP
        /// </summary>
        public static ElementModP NextElementModP(this Random random)
        {
            var array = random.NextInBoundsArray(ElementModP.MaxSize);
            return new ElementModP(array);
        }

        /// <summary>
        /// Generates a random ElementModQ
        /// </summary>
        public static ElementModQ NextElementModQ(this Random random)
        {
            var array = random.NextInBoundsArray(ElementModQ.MaxSize);
            return new ElementModQ(array);
        }

        /// <summary>
        /// Generates a random array of ulong values that are within the bounds of the specified size
        /// </summary>
        public static ulong[] NextInBoundsArray(this Random random, ulong size)
        {
            var array = new ulong[size];
            for (var i = 0; i < array.Length; i++)
            {
                // Generate a random ulong value using the Random class
                var value = ((uint)random.Next() << 32) | (uint)random.Next();
                array[i] = value;
            }
            return array;
        }
    }
}
