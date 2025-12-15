using System;
using System.Collections.Generic;
using Utility.Helpers.NonGeneric;

namespace Utility.Helpers
{
    public static class Comparison
    {
        public static bool IsGreaterThan<T>(this T value, T other) where T : IComparable
        {
            return value.CompareTo(other) > 0;
        }

        public static bool IsGreaterThanOrEqualTo<T>(this T value, T other) where T : IComparable
        {
            return value.CompareTo(other) >= 0;
        }

        public static bool IsLessThan<T>(this T value, T other) where T : IComparable
        {
            return value.CompareTo(other) < 0;
        }

        public static bool IsLessThanOrEqualTo<T>(this T value, T other) where T : IComparable
        {
            return value.CompareTo(other) <= 0;
        }

        public static T Max<T>(IEnumerable<T> all, Func<T, T, int> compare)
        {
            T max = default;
            int i = 0;
            foreach (var item in all)
            {
                if (i == 0)
                    max = item;
                else if (compare(item, max) > 0)
                    max = item;
                i++;
            }
            return max;
        }
    }
}