using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Helpers
{
    public enum SortOrder { Ascending, Descending }

    /// <summary>
    /// Centralized comparer factory with flexible options.
    /// </summary>
    public static class ComparerHelper
    {
        /// <summary>
        /// Creates a comparer from a key selector and sort order.
        /// </summary>
        public static IComparer<T> By<T, TKey>(
            Func<T, TKey> keySelector,
            SortOrder order = SortOrder.Ascending,
            IComparer<TKey>? keyComparer = null)
            where TKey : IComparable<TKey>
        {
            if(keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            keyComparer ??= Comparer<TKey>.Default;

            return new FuncComparer<T>((x, y) =>
            {
                var keyX = keySelector(x);
                var keyY = keySelector(y);
                return order == SortOrder.Ascending
                    ? keyComparer.Compare(keyX, keyY)
                    : keyComparer.Compare(keyY, keyX);
            });
        }

        /// <summary>
        /// Wraps an existing comparer to reverse the comparison.
        /// </summary>
        public static IComparer<T> Reverse<T>(IComparer<T> comparer)
        {
            return new FuncComparer<T>((x, y) => comparer.Compare(y, x));
        }

        /// <summary>
        /// Chains multiple comparers (compare by first, then second, etc.).
        /// </summary>
        public static IComparer<T> Chain<T>(params IComparer<T>[] comparers)
        {
            if (comparers == null || comparers.Length == 0)
                throw new ArgumentException("At least one comparer is required.", nameof(comparers));

            return new FuncComparer<T>((x, y) =>
            {
                foreach (var c in comparers)
                {
                    int result = c.Compare(x, y);
                    if (result != 0) return result;
                }
                return 0;
            });
        }

        /// <summary>
        /// Makes a comparer null-safe (nulls first or last).
        /// </summary>
        public static IComparer<T?> NullSafe<T>(IComparer<T> comparer, bool nullsFirst = true)
            where T : class
        {
            return new FuncComparer<T?>((x, y) =>
            {
                if (x is null && y is null) return 0;
                if (x is null) return nullsFirst ? -1 : 1;
                if (y is null) return nullsFirst ? 1 : -1;
                return comparer.Compare(x, y);
            });
        }

        /// <summary>
        /// Builds a chained comparer using fluent syntax.
        /// </summary>
        public static MultiComparer<T> Multi<T>() => new();
    }

    /// <summary>
    /// Simple comparer that delegates to a function.
    /// </summary>
    public class FuncComparer<T>(Func<T, T, int> comparer) : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

        public int Compare(T x, T y) => _comparer(x, y);
    }

    /// <summary>
    /// Fluent multi-criteria comparer builder.
    /// </summary>
    public class MultiComparer<T> : IComparer<T>
    {
        private readonly List<IComparer<T>> _comparers = new();

        public MultiComparer<T> Asc<TProp>(Func<T, TProp> keySelector) where TProp : IComparable<TProp>
        {
            _comparers.Add(ComparerHelper.By(keySelector, SortOrder.Ascending));
            return this;
        }

        public MultiComparer<T> Desc<TProp>(Func<T, TProp> keySelector) where TProp : IComparable<TProp>
        {
            _comparers.Add(ComparerHelper.By(keySelector, SortOrder.Descending));
            return this;
        }

        public int Compare(T x, T y)
        {
            foreach (var c in _comparers)
            {
                int result = c.Compare(x, y);
                if (result != 0) return result;
            }
            return 0;
        }
    }

    /*
    =====================
    EXAMPLE USAGE
    =====================

    var ageComparer = ComparerHelper.By<Person, int>(p => p.Age);
    var nameDescComparer = ComparerHelper.By<Person, string>(p => p.Name, SortOrder.Descending);
    var nullSafeName = ComparerHelper.NullSafe(nameDescComparer);
    var chained = ComparerHelper.Chain(ageComparer, nameDescComparer);

    var multi = ComparerHelper.Multi<Person>()
        .Asc(p => p.LastName)
        .Asc(p => p.FirstName)
        .Desc(p => p.Age);

    people.Sort(multi);
    */
}
