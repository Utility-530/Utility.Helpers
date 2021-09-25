using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public static class ArrayHelper
    {
        public static T[] To1dArray<T>(T[][] arr2d)
        {
            int width = arr2d[0].Length;
            int height = arr2d.Length;

            T[] arr1d = new T[width * height];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    arr1d[width * i + j] = arr2d[i][j];
                }

            return arr1d;
        }

        public static T[,] TrimArray<T>(this T[,] originalArray, int rowToRemove, int columnToRemove)
        {
            T[,] result = new T[originalArray.GetLength(0) - 1, originalArray.GetLength(1) - 1];

            for (int i = 0, j = 0; i < originalArray.GetLength(0); i++)
            {
                if (i == rowToRemove)
                    continue;

                for (int k = 0, u = 0; k < originalArray.GetLength(1); k++)
                {
                    if (k == columnToRemove)
                        continue;

                    result[j, u] = originalArray[i, k];
                    u++;
                }
                j++;
            }

            return result;
        }

        public static T[,] ToMultiDimensionalArray<T>(this Dictionary<(string, string), T> dictionary, IComparer<string>? comparer = null) where T : struct
        {
            comparer ??= StringComparer.InvariantCultureIgnoreCase;

            var hNames = dictionary.Select(c => c.Key.Item1).Distinct().OrderBy(a => a, comparer).ToArray();
            var vNames = dictionary.Select(c => c.Key.Item2).Distinct().OrderBy(a => a, comparer).ToArray();

            return ToMultiDimensionalValueArray(dictionary.ToDictionary(a => a.Key, a => (T)a.Value), hNames, vNames);
        }

        public static T?[,] ToMultiDimensionalArray<T>(this Dictionary<(string, string), T> dictionary, string[] hNames, string[] vNames) where T : class
        {
            T?[,] result = new T[hNames.Length, vNames.Length];

            for (int i = 0; i < hNames.Length; i++)
            {
                for (int k = 0; k < vNames.Length; k++)
                {
                    if (dictionary.ContainsKey((hNames[i], vNames[k])))
                    {
                        result[i, k] = dictionary[(hNames[i], vNames[k])];
                    }
                    else
                    {
                        result[i, k] = default;
                    }
                }
            }

            return result;
        }

        public static T[,] ToMultiDimensionalValueArray<T>(this Dictionary<(string, string), T> dictionary, string[] hNames, string[] vNames) where T : struct
        {
            T[,] result = new T[hNames.Length, vNames.Length];

            for (int i = 0; i < hNames.Length; i++)
            {
                for (int k = 0; k < vNames.Length; k++)
                {
                    if (dictionary.ContainsKey((hNames[i], vNames[k])))
                    {
                        result[i, k] = dictionary[(hNames[i], vNames[k])];
                    }
                    else
                    {
                        result[i, k] = default;
                    }
                }
            }

            return result;
        }

        public static T[,] ToMultidimensionalArray<T>(this IList<T[]?> arrays)
        {
            var minorLength = arrays.Max(c => c?.Length ?? 0);
            var ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                T[]? array = arrays[i];
                if (array != null)
                    for (int j = 0; j < array.Length; j++)
                    {
                        ret[i, j] = array[j];
                    }
            }
            return ret;
        }

        public static T[][] RemoveColumns<T>(this T[][] originalArray, params int[] columnsToRemove)
        {
            T[][] result = new T[originalArray.GetLength(0)][];

            for (int i = 0; i < originalArray.GetLength(0); i++)
            {
                result[i] = new T[originalArray[i].Length - columnsToRemove.Count()];
                for (int k = 0, u = 0; k < originalArray[i].Length; k++)
                {
                    if (!columnsToRemove.Contains(k))
                    {
                        result[i][u] = originalArray[i][k];
                        u++;
                    }
                }
            }

            return result;
        }

        public static T[][] RemoveColumns<T>(this T[][] originalArray, params T[][] columnsToRemove)
        {
            int l2 = originalArray[0].Length;

            var ints = columnsToRemove
                .Select(ctr =>
                {
                    var cols = Enumerable.Range(0, l2).Single(i => originalArray.Select(ar => ar[i]).All(a => a!.Equals(ctr[i])));
                    return cols;
                });
            return RemoveColumns(originalArray, ints.ToArray());
        }
    }
}