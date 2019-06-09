using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public static class LinqExtension
    {

        public static IEnumerable<(T, T)> LeftOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality) => from first in firsts
                                                                                                                                        join last in lasts on equality(first) equals equality(last) into temp
                                                                                                                                        from last in temp.DefaultIfEmpty()
                                                                                                                                        select (first, last);

        public static IEnumerable<(T, T)> RightOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality) => from last in lasts
                                                                                                                                         join first in firsts on equality(last) equals equality(first) into temp
                                                                                                                                         from first in temp.DefaultIfEmpty()
                                                                                                                                         select (first, last);

        public static IEnumerable<(T, T)> FullOuterJoin<T, R>(this IEnumerable<T> firsts, IEnumerable<T> lasts, Func<T, R> equality) => LeftOuterJoin(firsts, lasts, equality).Concat(RightOuterJoin(firsts, lasts, equality));


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
    }
}
