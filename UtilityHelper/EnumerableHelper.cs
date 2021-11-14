using Endless.Functional;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class EnumerableHelper
    {
        public static T? GetNext<T>(this IEnumerator<T> enumerator) where T : struct
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }

        //public static (bool hasValue, T value) GetNextRef<T>(this IEnumerator<T> enumerator) where T : class
        //{
        //    return enumerator.MoveNext().Pipe(a => (a, a ? enumerator.Current : null));
        //}

        public static T? TryGetNext<T>(this IEnumerator<T> enumerator) where T : struct
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static NullableRef<T> GetNextRefSafe<T>(this IEnumerator<T> enumerator) where T : class
        {
            return enumerator.MoveNext().Pipe(a => a ? new NullableRef<T>(enumerator.Current, true) : new NullableRef<T>(default, false));
        }
    }

    public record NullableRef<T>(T? Value, bool HasValue) where T : class;
}
