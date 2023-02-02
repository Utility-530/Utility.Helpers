using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Helpers.Generic
{
    public static class LinqEx
    {
        public static T FindLast<T>(IEnumerable<T> items, Func<T, DateTime> f, DateTime dt)
        {
            return items.Where(pss => f(pss) < dt).MaxBy(ps => f(ps)).First();
        }

        public static void RemoveLast<T>(this ICollection<T> collection, int n)
        {
            var x = collection.TakeLast(n);
            foreach (var y in x)
            {
                collection.Remove(y);
            }
        }

        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> s, Func<T, U> f)
        {
            foreach (var item in s)
                yield return f(item);
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
        {
            // argument null checking omitted
            int i = 0;
            foreach (T item in sequence)
            {
                action(item, i);
                i++;
            }
        }

        public static void AddOrReplaceBy<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector, TSource replacement)
        {
            RemoveBy(source, keySelector, keySelector(replacement));
            source.Add(replacement);
        }

        public static void RemoveBy<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector, TKey key)
        {
            source.ActionBy(keySelector, key, (a, b) => a.Remove(b));
        }

        public static void ActionBy<TSource, TKey>(this ICollection<TSource> source, Func<TSource, TKey> keySelector, TKey key, Action<ICollection<TSource>, TSource> action)
        {
            if (!source.IsEmpty())
                foreach (TSource element in source.ToList())
                    if (key?.Equals(keySelector(element)) ?? false)
                    {
                        action(source, element);
                    }
        }

        public static TSource Pernultimate<TSource>(this IEnumerable<TSource> source)
        {
            //from http://stackoverflow.com/questions/8724179/linq-how-to-get-second-last
            return source.Reverse().Skip(1).Take(1).FirstOrDefault();
        }

        public static Boolean IsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true; // or throw an exception
            return !source.Any();
        }

        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n)
        {
            return source.Skip(Math.Max(0, source.Count() - n));
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> collection, int size)
        {
            Func<IEnumerable<T>, int> f = (c) => c.Count() / size;

            var chunkCount = collection.Count() % size > 0 ? f(collection) + 1 : f(collection);

            return Enumerable.Range(0, chunkCount).Select(i => collection.Skip(i * size).Take(size).ToList());
        }

        public static IEnumerable<T>[] SplitInTwo<T>(this IEnumerable<T> collection, double ratio)
        {
            int chunkCount = (int)(collection.Count() * ratio);

            return new[] { collection.Take(chunkCount), collection.Skip(chunkCount) };
        }

        // zip multiple collections
        public static IEnumerable<TResult> Zip<TResult>(Func<object[], TResult> resultSelector,
params System.Collections.IEnumerable[] itemCollections)
        {
            System.Collections.IEnumerator[] enumerators = itemCollections.Select(i => i.GetEnumerator()).ToArray();

            Func<bool> MoveNext = () =>
            {
                for (int i = 0; i < enumerators.Length; i++)
                {
                    if (!enumerators[i].MoveNext())
                    {
                        return false;
                    }
                }
                return true;
            };

            while (MoveNext())
            {
                yield return resultSelector(enumerators.Select(e => e.Current).ToArray());
            }
        }

        public static void RemoveFirst<T>(this ICollection<T> collection, int n)
        {
            var x = collection.Take(n);
            foreach (var y in x)
            {
                collection.Remove(y);
            }
        }

        // From MoreLinq
        public static IEnumerable<TSource> MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            comparer ??= Comparer<TKey>.Default;
            return ExtremaBy(source, selector, (x, y) => comparer.Compare(x, y));
        }

        // From MoreLinq
        public static IEnumerable<TSource> MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            comparer ??= Comparer<TKey>.Default;
            return ExtremaBy(source, selector, (x, y) => -Math.Sign(comparer.Compare(x, y)));
        }

        // > In mathematical analysis, the maxima and minima (the respective
        // > plurals of maximum and minimum) of a function, known collectively
        // > as extrema (the plural of extremum), ...
        // >
        // > - https://en.wikipedia.org/wiki/Maxima_and_minima

        private static IEnumerable<TSource> ExtremaBy<TSource, TKey>(IEnumerable<TSource> source,
            Func<TSource, TKey> selector, Func<TKey, TKey, int> comparer)
        {
            foreach (var item in Extrema())
                yield return item;

            IEnumerable<TSource> Extrema()
            {
                using (var e = source.GetEnumerator())
                {
                    if (!e.MoveNext())
                        return new List<TSource>();

                    var extrema = new List<TSource> { e.Current };
                    var extremaKey = selector(e.Current);

                    while (e.MoveNext())
                    {
                        var item = e.Current;
                        var key = selector(item);
                        var comparison = comparer(key, extremaKey);
                        if (comparison > 0)
                        {
                            extrema = new List<TSource> { item };
                            extremaKey = key;
                        }
                        else if (comparison == 0)
                        {
                            extrema.Add(item);
                        }
                    }

                    return extrema;
                }
            }
        }

        public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight, double control = 0)
        {
            double weightedValueSum = records.Sum(x => (value(x) - control) * weight(x));
            double weightSum = records.Sum(x => weight(x));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                return 0;// throw new DivideByZeroException("No weights are greater than 0");
        }

        public static IEnumerable<double> RunningWeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight)
        {
            var runningweightedvaluesum = 0d;
            var runningweightsum = 0d;
            foreach (var x in records)
            {
                runningweightedvaluesum += value(x) * weight(x);
                runningweightsum += weight(x);
                if (runningweightedvaluesum != 0)
                    yield return runningweightedvaluesum / runningweightsum;
                else
                    yield return 0;
            }
        }

        public static IEnumerable<TResult> TakeIfNotNull<TResult>(this IEnumerable<TResult> source, int? count)
        {
            return !count.HasValue ? source : source.Take(count.Value);
        }

        public static IEnumerable<TResult> TakeAllIfNull<TResult>(this IEnumerable<TResult> source, int? count)
        {
            if (count == null)
                return source;
            else
                return source.Take(count.Value);
        }

        public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
    this IEnumerable<TSource> source,
    TAccumulate seed,
    Func<TAccumulate, TSource, TAccumulate> func)
        {
            //source.CheckArgumentNull("source");
            //func.CheckArgumentNull("func");
            return source.SelectAggregateIterator(seed, func);
        }

        private static IEnumerable<TAccumulate> SelectAggregateIterator<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func)
        {
            TAccumulate previous = seed;
            foreach (var item in source)
            {
                TAccumulate result = func(previous, item);
                previous = result;
                yield return result;
            }
        }

        public static IEnumerable<IGrouping<object, T>> Pivot<T>(this IEnumerable<T> dtable, string RowField, string DataField, string columnField)
        {
            var rprop = typeof(T).GetProperty(RowField);
            var cprop = typeof(T).GetProperty(columnField);

            var query = dtable
                   .GroupBy(r => rprop.GetValue(r, null))
                   .GroupBy(c => cprop.GetValue(c, null))
                   .Select(a => a.First());

            return query;
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacent<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            where TKey : notnull
        {
            TKey last = default;
            bool haveLast = false;
            List<TSource> list = new List<TSource>();
            foreach (TSource s in source)
            {
                TKey k = keySelector(s);
                if (haveLast)
                {
                    if (!k.Equals(last))
                    {
                        last = k;
                        yield return new GroupOfAdjacent<TSource, TKey>(list, last);
                        list = new List<TSource> { s };
                    }
                    else
                    {
                        list.Add(s);
                        last = k;
                    }
                }
                else
                {
                    list.Add(s);
                    last = k;
                    haveLast = true;
                }
            }
            if (haveLast)
                yield return new GroupOfAdjacent<TSource, TKey>(list, last!);
        }
    }

    public class GroupOfAdjacent<TSource, TKey> : IEnumerable<TSource>, IGrouping<TKey, TSource>
    {
        public TKey Key { get; set; }
        private List<TSource> GroupList { get; set; }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.Generic.IEnumerable<TSource>)this).GetEnumerator();
        }

        System.Collections.Generic.IEnumerator<TSource> System.Collections.Generic.IEnumerable<TSource>.GetEnumerator()
        {
            foreach (var s in GroupList)
                yield return s;
        }

        public GroupOfAdjacent(List<TSource> source, TKey key)
        {
            GroupList = source;
            Key = key;
        }
    }
}