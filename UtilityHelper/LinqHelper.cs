using Endless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public static class LinqHelper
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Any() == false;
        }

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
        public static T1 AggregateUntil<T1,T2>(this IEnumerable<T2> enumerable,
            T1 seed,
            Func<T1, T2, T1> func, 
            Func<T1,bool> predicate)
        {
            return enumerable
                .Scan(seed, func)
                .TakeUntil(predicate)
                .Last();
        }
    }
}
