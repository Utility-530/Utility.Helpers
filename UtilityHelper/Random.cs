using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public static class RandomHelper
    {
        private static Lazy<Random> random = LazyEx.Create<Random>();

        public static IEnumerable<T> Sample<T>(this IEnumerable<T> x, int count, Random rand) => x.OrderBy(_ => rand.Next()).Take(count);

        public static IEnumerable<T> Sample<T>(this IEnumerable<T> x, int count) => x.OrderBy(arg => Guid.NewGuid()).Take(count);

        public static IEnumerable<T> SampleOrdered<T>(this IEnumerable<T> x, double percent, Random rand = null)
        {
            if (percent <= 0) throw new Exception("percent must be greater than 0");
            if (percent >= 1) throw new Exception("percent must be less than 1");

            rand = rand ?? random.Value;

            using (var e = x.GetEnumerator())
            {
                while (e.MoveNext())
                    if (rand.Next() <= percent)
                        yield return e.Current;
            }
        }

        public static IEnumerable<T> SampleOrdered<T>(this ICollection<T> x, int count, Random rand = null) => x.SampleOrdered(count / (double)x.Count, rand).Take(count);

        public static IEnumerable<int> SampleInRange(int size, int percent, Random rand = null)
        {
            if (percent <= 0) throw new Exception("percent must be greater than 0");
            if (percent >= 100) throw new Exception("percent must be less than 100");

            rand = rand ?? random.Value;

            for (int i = 0; i < size; i++)
            {
                if (rand.Next(100) > percent)
                {
                    yield return i;
                }
            }
        }

        public static bool NextBoolean(this Random source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.NextDouble() > 0.5;
        }

        private static string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
        private static string[] vowels = { "a", "e", "i", "o", "u" };

        public static string NextWord(int length = 4, Random rand = null)
        {
            rand = rand ?? random.Value;

            if (length < 1) // do not allow words of zero length
                throw new ArgumentException("Length must be greater than 0");

            string word = string.Empty;

            if (rand.Next() % 2 == 0) // randomly choose a vowel or consonant to start the word
                word += consonants[rand.Next(0, 20)];
            else
                word += vowels[rand.Next(0, 4)];

            for (int i = 1; i < length; i += 2) // the counter starts at 1 to account for the initial letter
            { // and increments by two since we append two characters per pass
                string c = consonants[rand.Next(0, 20)];
                string v = vowels[rand.Next(0, 4)];

                if (c == "q") // append qu if the random consonant is a q
                    word += "qu";
                else // otherwise just append a random consant and vowel
                    word += c + v;
            }

            // the word may be short a letter because of the way the for loop above is constructed
            if (word.Length < length) // we'll just append a random consonant if that's the case
                word += consonants[rand.Next(0, 20)];

            return word;
        }

        public const double Epsilon = 0.000001;

        public static int NextSign(this Random random, int factor = 1)
        {
            var result = random.Next(0, 2) * 2 - 1;
            return factor * result;
        }

        public static double NextSignDouble(this Random random, double factor = 1d)
        {
            var result = random.Next(0, 2) * 2 - 1;
            return factor * result * random.NextDouble();
        }

        public static double NextValue(double current, Random random, double min, double max)
        {
            double increment = random.NextDouble() / 50;
            double sign = random.NextSign();
            if (sign > 0)
            {
                current += increment;
                while (current > max)
                    current -= increment;
            }
            else
            {
                current -= increment;
                while (current < min)
                    current += increment;
            }

            return current;
        }

        public static T NextEnumValue<T>(Random random, Dictionary<Type, Array>? cache = default) where T : Enum
        {
            if (cache?.ContainsKey(typeof(T)) == false)
            {
                cache[typeof(T)] = Enum.GetValues(typeof(T));
            }

            var a = GetArray(cache);

            return (T)a.GetValue(random.Next(a.Length));

            static Array GetArray(Dictionary<Type, Array>? cache)
            {
                return cache != null ? cache[typeof(T)] : Enum.GetValues(typeof(T));
          }
        }

        public static double NextLevyValue(Random random, double c = 5.5, double mu = 1)
        {
            double u, v, t, s;

            u = Math.PI * (random.NextDouble() - 0.5);

            // the Cauchy case
            if (Math.Abs(mu - 1) < Epsilon)
            {
                t = Math.Tan(u);
                return c * t;
            }

            do
            {
                v = -Math.Log(random.NextDouble());
            } while (Math.Abs(v - 0) < Epsilon);

            // the Gaussian case
            if (Math.Abs(mu - 2) < Epsilon)
            {
                t = 2 * Math.Sin(u) * Math.Sqrt(v);
                return c * t;
            }

            // the general case
            t = Math.Sin(mu * u) / Math.Pow(Math.Cos(u), 1 / mu);
            s = Math.Pow(Math.Cos((1 - mu) * u) / v, (1 - mu) / mu);

            return c * t * s;
        }
    }
}