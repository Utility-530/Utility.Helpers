using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public static class SampleHelper
    {
        public static IEnumerable<T> Sample<T>(this Random rand, IEnumerable<T> population, int count) => population.OrderBy(a => rand.Next()).Take(count);

        public static IEnumerable<T> Sample<T>(this IEnumerable<T> population, int count) => population.OrderBy(arg => Guid.NewGuid()).Take(count);

        public static IEnumerable<T> SampleOrdered<T>(this Random random, IEnumerable<T> x, double percent)
        {
            if (percent <= 0) throw new Exception("percent must be greater than 0");
            if (percent >= 1) throw new Exception("percent must be less than 1");


            using var e = x.GetEnumerator();
            while (e.MoveNext())
                if (random.Next() <= percent)
                    yield return e.Current;
        }

        public static IEnumerable<T> SampleOrdered<T>(this Random random, ICollection<T> population, int count) => random.SampleOrdered(population, count / (double)population.Count).Take(count);

        public static IEnumerable<int> SampleInRange(this Random random, int size, int percent)
        {
            if (percent <= 0) throw new Exception("percent must be greater than 0");
            if (percent >= 100) throw new Exception("percent must be less than 100");


            for (int i = 0; i < size; i++)
            {
                if (random.Next(100) > percent)
                {
                    yield return i;
                }
            }
        }
    }
}
