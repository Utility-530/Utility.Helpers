using System;
using System.Collections.Generic;

namespace Utility.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Create<T>(int count, Func<T> create)
        {
            for (int i = 0; i < count; i++)
            {
                yield return create();
            }
        }
    }
}