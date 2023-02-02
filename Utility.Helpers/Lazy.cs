using System;

namespace Utility.Helpers
{
    public static class LazyEx
    {
        public static Lazy<T> Create<T>(this Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return new Lazy<T>(func);
        }

        public static Lazy<T> Create<T>() where T : new()
        {
            T t() => new T();

            return new Lazy<T>(t);
        }
    }
}