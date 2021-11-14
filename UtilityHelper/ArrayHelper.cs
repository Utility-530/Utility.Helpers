using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public static class ArrayHelper
    {
        public static T?[,] ToMultidimensionalArray<T>(this IList<T[]?> arrays)
        {
            var minorLength = arrays.Max(c => c?.Length ?? 0);
            var ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                var array = arrays[i];

                for (int j = 0; j < (array?.Length ?? 0); j++)
                {
                    ret[i, j] = array![j];
                }
            }
            return ret;
        }
    }
}