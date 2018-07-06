using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityHelper
{
    public class ArrayHelper
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

    }
}
