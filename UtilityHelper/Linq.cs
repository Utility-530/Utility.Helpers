using System;
using System.Collections.Generic;
using System.Linq;
using UtilityHelper.Generic;

namespace UtilityHelper
{
    public static class LinqExtension
    {
        public static IEnumerable<(T, T)> LeftOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> seconds, Func<T, R> equality)
            => from first in firsts
               join second in seconds on equality(first) equals equality(second) into temp
               from a in temp.DefaultIfEmpty()
               select (first, a);

        public static IEnumerable<(T, R)> LeftOuterJoin<T, R, S>(this IEnumerable<T> firsts, IEnumerable<R> seconds, Func<T, S> equalityOne, Func<R, S> equalityTwo)
            => from first in firsts
               join second in seconds on equalityOne(first) equals equalityTwo(second) into temp
               from a in temp.DefaultIfEmpty()
               select (first, a);

        public static IEnumerable<(T, T)> RightOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> seconds, Func<T, R> equality)
            => from second in seconds
               join first in firsts on equality(second) equals equality(first) into temp
               from a in temp.DefaultIfEmpty()
               select (a, second);

        public static IEnumerable<(T, R)> RightOuterJoin<T, R, S>(this IEnumerable<T> firsts, IEnumerable<R> seconds, Func<T, S> equalityOne, Func<R, S> equalityTwo)
            => from second in seconds
               join first in firsts on equalityTwo(second) equals equalityOne(first) into temp
               from a in temp.DefaultIfEmpty()
               select (a, second);

        public static IEnumerable<(T, T)> FullOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> seconds, Func<T, R> equality)
            => LeftOuterJoin(firsts, seconds, equality).Concat(RightOuterJoin(firsts, seconds, equality));

        public static IEnumerable<(T, R)> FullOuterJoin<T, R, S>(this IEnumerable<T> firsts, IEnumerable<R> lasts, Func<T, S> keySelector1, Func<R, S> keySelector2)
            => LeftOuterJoin(firsts, lasts, keySelector1, keySelector2).Concat(RightOuterJoin(firsts, lasts, keySelector1, keySelector2));

        /// <summary>
        /// Selects the differences between values in <see cref="sequence"/>
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IEnumerable<double> SelectDifferences(this IEnumerable<double> sequence)
        {
            using (var e = sequence.GetEnumerator())
            {
                e.MoveNext();
                double last = e.Current;
                while (e.MoveNext())
                    yield return e.Current - last;
            }
        }

        /// <summary>
        /// Selects all items in <see cref="first"/> that are not in <see cref="second"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<T> SelectFromFirstNotInSecond<T>(IEnumerable<T> first, IEnumerable<T> second)
            => from n in first
               join n2 in second
               on n equals n2 into temp
               where temp.Count() == 0
               select n;

        /// <summary>
        /// Selects all items in <see cref="first"/> that are not in <see cref="second"/> using keySelectors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<T> SelectFromFirstNotInSecond<T, R, S>(IEnumerable<T> first, IEnumerable<S> second, Func<T, R> keySelectorFirst, Func<S, R> keySelectorSecond)
            => from n in first
               join n2 in second
               on keySelectorFirst(n) equals keySelectorSecond(n2) into temp
               where temp.Count() == 0
               select n;

        /// <summary>
        /// Selects all items in <see cref="first"/> that are not in <see cref="second"/> using keySelectors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<T> FilterFirstNotInSecond<T, R>(IEnumerable<T> first, IEnumerable<R> second, Func<T, R> keySelectorFirst)

                => from n in first
                   join n2 in second
                   on keySelectorFirst(n) equals n2 into temp
                   where temp.Count() == 0
                   select n;


        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable.Any() == false;

        /// <summary>
        /// Aggregate <see cref="enumerable"/> based on <see cref="func"/> until a condition,<see cref="predicate"/>, is met.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="seed"></param>
        /// <param name="func"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T1 AggregateUntil<T1, T2>(this IEnumerable<T2> enumerable,
            T1 seed,
            Func<T1, T2, T1> func,
            Func<T1, bool> predicate)
        {
            return enumerable
                .Scan(seed, func)
                .TakeWhile(a => predicate(a) != true)
                .Last();
        }
    }
}