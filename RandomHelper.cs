using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class RandomHelper
    {

        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, double percent, Random rand = null)
        {
            rand = rand ?? new Random();

            using (var e = x.GetEnumerator())
            {
                while (e.MoveNext())
                    if (rand.Next(100) < percent)
                        yield return e.Current;

            }


        }
        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, int count, Random rand = null)
        {

            return x.OrderBy(_ => rand.Next()).Take(count);

        }

        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> x, int count)
        {

            return x.OrderBy(arg => Guid.NewGuid()).Take(count);
        }



        public static IEnumerable<int> GetRandomNumbersInRange(int size, int percent, Random rand = null)
        {
            rand = rand ?? new Random();

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




        static string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
        static string[] vowels = { "a", "e", "i", "o", "u" };

        public static string NextWord(int length = 4,Random rand = null)
        {
            rand = rand ?? new Random();

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
    }

}