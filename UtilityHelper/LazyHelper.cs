using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class LazyHelper
    {
        public static Lazy<T> Create<T>(Func<T> func)
        {
            return new Lazy<T>(func);
        }

        public static Lazy<T> ToLazy<T>(this Func<T> func)
        {
            return new Lazy<T>(func);
        }
    }
}
