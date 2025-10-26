using System;
using System.Collections.Generic;

namespace Utility.Helpers
{
    /// <summary>
    /// Static helper class for creating IComparer implementations
    /// </summary>
    public static class ComparerHelper
    {
        /// <summary>
        /// Creates a comparer from a comparison delegate
        /// </summary>
        public static IComparer<T> Create<T>(Comparison<T> comparison)
        {
            return new DelegateComparer<T>(comparison);
        }

        /// <summary>
        /// Creates a comparer from a key selector function
        /// </summary>
        public static IComparer<T> CreateByKey<T, TKey>(Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            return new KeyComparer<T, TKey>(keySelector);
        }

        /// <summary>
        /// Creates a comparer from a key selector with custom key comparer
        /// </summary>
        public static IComparer<T> CreateByKey<T, TKey>(Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
        {
            return new KeyComparer<T, TKey>(keySelector, keyComparer);
        }

        /// <summary>
        /// Creates a reversed comparer
        /// </summary>
        public static IComparer<T> Reverse<T>(IComparer<T> comparer)
        {
            return new ReverseComparer<T>(comparer);
        }

        /// <summary>
        /// Creates a comparer that chains multiple comparers (compares by first, then by second, etc.)
        /// </summary>
        public static IComparer<T> Chain<T>(params IComparer<T>[] comparers)
        {
            return new ChainedComparer<T>(comparers);
        }

        /// <summary>
        /// Creates a null-safe comparer (nulls are ordered first by default)
        /// </summary>
        public static IComparer<T> NullSafe<T>(IComparer<T> comparer, bool nullsFirst = true)
        {
            return new NullSafeComparer<T>(comparer, nullsFirst);
        }

        #region Comparer Implementations

        private class DelegateComparer<T> : IComparer<T>
        {
            private readonly Comparison<T> _comparison;

            public DelegateComparer(Comparison<T> comparison)
            {
                _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }
        }

        private class KeyComparer<T, TKey> : IComparer<T>
        {
            private readonly Func<T, TKey> _keySelector;
            private readonly IComparer<TKey> _keyComparer;

            public KeyComparer(Func<T, TKey> keySelector)
                : this(keySelector, Comparer<TKey>.Default)
            {
            }

            public KeyComparer(Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
            {
                _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
                _keyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
            }

            public int Compare(T x, T y)
            {
                var keyX = _keySelector(x);
                var keyY = _keySelector(y);
                return _keyComparer.Compare(keyX, keyY);
            }
        }

        private class ReverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _innerComparer;

            public ReverseComparer(IComparer<T> innerComparer)
            {
                _innerComparer = innerComparer ?? throw new ArgumentNullException(nameof(innerComparer));
            }

            public int Compare(T x, T y)
            {
                return _innerComparer.Compare(y, x); // Reversed
            }
        }

        private class ChainedComparer<T> : IComparer<T>
        {
            private readonly IComparer<T>[] _comparers;

            public ChainedComparer(IComparer<T>[] comparers)
            {
                _comparers = comparers ?? throw new ArgumentNullException(nameof(comparers));
                if (_comparers.Length == 0)
                    throw new ArgumentException("At least one comparer is required", nameof(comparers));
            }

            public int Compare(T x, T y)
            {
                foreach (var comparer in _comparers)
                {
                    int result = comparer.Compare(x, y);
                    if (result != 0)
                        return result;
                }
                return 0;
            }
        }

        private class NullSafeComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _innerComparer;
            private readonly bool _nullsFirst;

            public NullSafeComparer(IComparer<T> innerComparer, bool nullsFirst)
            {
                _innerComparer = innerComparer ?? throw new ArgumentNullException(nameof(innerComparer));
                _nullsFirst = nullsFirst;
            }

            public int Compare(T x, T y)
            {
                if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
                    return 0;

                if (ReferenceEquals(x, null))
                    return _nullsFirst ? -1 : 1;

                if (ReferenceEquals(y, null))
                    return _nullsFirst ? 1 : -1;

                return _innerComparer.Compare(x, y);
            }
        }

        #endregion
    }

    #region Example Usage

    /*
    // Example 1: Simple comparison delegate
    var ageComparer = ComparerHelper.Create<Person>((p1, p2) => p1.Age.CompareTo(p2.Age));

    // Example 2: Compare by key
    var nameComparer = ComparerHelper.CreateByKey<Person, string>(p => p.Name);

    // Example 3: Reverse sorting
    var reverseAgeComparer = ComparerHelper.Reverse(ageComparer);

    // Example 4: Chain comparers (sort by Name, then by Age)
    var chainedComparer = ComparerHelper.Chain(
        ComparerHelper.CreateByKey<Person, string>(p => p.Name),
        ComparerHelper.CreateByKey<Person, int>(p => p.Age)
    );

    // Example 5: Null-safe comparison
    var nullSafeComparer = ComparerHelper.NullSafe(nameComparer, nullsFirst: true);

    // Example 6: Complex chaining with reverse
    var complexComparer = ComparerHelper.Chain(
        ComparerHelper.CreateByKey<Person, string>(p => p.Department),
        ComparerHelper.Reverse(ComparerHelper.CreateByKey<Person, decimal>(p => p.Salary)),
        ComparerHelper.CreateByKey<Person, string>(p => p.Name)
    );

    // Usage in sorting
    var people = new List<Person> { ... };
    people.Sort(chainedComparer);

    // Usage with SortedSet
    var sortedPeople = new SortedSet<Person>(ageComparer);

    // Usage with LINQ
    var sortedList = people.OrderBy(p => p, nameComparer).ToList();
    */

    #endregion
}