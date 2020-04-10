using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityHelper
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
    }
}
