using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public static class LinqExtension
    {

        public static IEnumerable<(T, T)> LeftOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality)
            => from first in firsts
               join last in lasts on equality(first) equals equality(last) into temp
               from last in temp.DefaultIfEmpty()
               select (first, last);

        public static IEnumerable<(T, T)> RightOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality)
            => from last in lasts
               join first in firsts on equality(last) equals equality(first) into temp
               from first in temp.DefaultIfEmpty()
               select (first, last);

        public static IEnumerable<(T, T)> FullOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality)
            => LeftOuterJoin(firsts, lasts, equality).Concat(RightOuterJoin(firsts, lasts, equality));

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


    }
}
