using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityModel;

namespace UtilityHelper
{

    /// <summary>
    /// Collection of methods that utilise the metaphone algorithm
    /// </summary>
    public static class StringSimilarity
    {

        public static bool IsSimilarToAny(this string str, params string[] strings)
        {
            return strings.Any(s => s.IsSimilarTo(str));
        }

        public static bool IsSimilarToAll(this string str, params string[] strings)
        {
            return strings.All(s => s.IsSimilarTo(str));
        }

        public static bool IsSimilarTo(this string str, string otherStr)
        {
            var m = new Metaphone();
            return m.Encode(str) == m.Encode(otherStr);
        }


        public static string[] SimilarWords(this string str, string otherStr, bool caseSensitive = false, string splitBy = " ", int minWordLength = 2, bool includeMistyped = true)
        {
            if (caseSensitive)
            {
                str = str.ToLower();
                otherStr = otherStr.ToLower();
            }

            var str1Arr = str.Split(splitBy).Where(w => w.Length >= minWordLength).ToArray();
            var str2Arr = otherStr.Split(splitBy).Where(w => w.Length >= minWordLength).ToArray();

            var m = new Metaphone();
            var metaphoneStr1Arr = str1Arr.Select(w => m.Encode(w)).ToArray();
            var metaphoneStr2Arr = str2Arr.Select(w => m.Encode(w)).ToArray();
            var metaphoneIntersection = metaphoneStr1Arr.Intersect(metaphoneStr2Arr).ToArray();

            var mistypedIntersection = new List<string>();

            if (includeMistyped) // uwaga, przy takim porównywaniu wyjdzie, że II is similar to Munich
                foreach (var s1 in str1Arr)
                    foreach (var s2 in str2Arr)
                        if (Math.Abs(s1.Length - s2.Length) <= 2 && (s1.ContainsAll(s2.Select(c => c.ToString()).ToArray()) || s2.ContainsAll(s1.Select(c => c.ToString()).ToArray())))
                            mistypedIntersection.Add(m.Encode(s1));

            return metaphoneIntersection.Concat(mistypedIntersection).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
        }

        public static string[] SimilarWords(this string str, string[] otherStrings, bool caseSensitive, string splitBy = " ", int minWordLength = 2)
        {
            var sameWords = new List<string>();

            foreach (var otherStr in otherStrings)
                sameWords.AddRange(str.SimilarWords(otherStr, caseSensitive, splitBy, minWordLength));

            return sameWords.Distinct().ToArray();
        }

        public static string[] SimilarWords(this string str, params string[] otherStrings)
        {
            return str.SimilarWords(otherStrings, false, " ", 2);
        }

        public static bool HasSimilarWords(this string str, string otherStr, bool caseSensitive = false, string splitBy = " ", int minWordLength = 2)
        {
            return str.SimilarWords(otherStr, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSimilarWords(this string str, string[] otherStrings, bool caseSensitive, string splitBy = " ", int minWordLength = 2)
        {
            return str.SimilarWords(otherStrings, caseSensitive, splitBy, minWordLength).Any();
        }

        public static bool HasSimilarWords(this string str, params string[] otherStrings)
        {
            return str.SimilarWords(otherStrings, false, " ", 2).Any();
        }



    }
}
