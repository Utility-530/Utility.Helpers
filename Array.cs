using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            //T[][] result = new T[originalArray.GetLength(0)][];

            int l2 = originalArray[0].Length;

            var ints = columnsToRemove.Select(ctr =>
            {
                try
                {
                    var cols = Enumerable.Range(0, l2).Single(i => originalArray.Select(ar => ar[i]).All(_ => _.Equals(ctr[i])));
                    return cols;
                }
                catch
                {

                    throw new Exception();
                }
            }
            );
            return RemoveColumns(originalArray, ints.ToArray());

        }



    }
}
