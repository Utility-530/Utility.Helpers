using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Utility.Helpers.Reflection
{
    /// <summary>
    /// <a href="https://stackoverflow.com/questions/6427861/determining-whether-a-struct-is-of-default-value-without-equals-aka-reference"></a>
    /// </summary>
    public class Comparison
    {
        private static readonly ConcurrentDictionary<Type, Func<object, bool>> defaultValueComparisons =
  new ConcurrentDictionary<Type, Func<object, bool>>();

        // Essentially unchanged from Eamon Nerbonne's version
        public static bool IsDefaultValue(object? value, bool isNullTreatedAsDefaultValue = true)
        {
            if (value == null) return isNullTreatedAsDefaultValue;

            Type type = value.GetType();

            return type.IsValueType &&
                   defaultValueComparisons.GetOrAdd(
                     type,
                     t =>
                     {
                         var method = typeof(StructHelpers<>)
                                        .MakeGenericType(t)
                                        .GetMethod(nameof(StructHelpers<int>.IsDefaultValue));
                         var objParam = Expression.Parameter(typeof(object), "obj");
                         var call = Expression.Call(method, Expression.Convert(objParam, t));
                         return Expression.Lambda<Func<object, bool>>(call, objParam).Compile();
                     }).Invoke(value);
        }

        private static class StructHelpers<T> where T : struct
        {
            // ReSharper disable StaticMemberInGenericType
            private static readonly int ByteCount = Unsafe.SizeOf<T>();

            private static readonly int LongCount = ByteCount / 8;
            private static readonly int ByteRemainder = ByteCount % 8;
            // ReSharper restore StaticMemberInGenericType

            public static bool IsDefaultValue(T a)
            {
                if (LongCount > 0)
                {
                    ref long p = ref Unsafe.As<T, long>(ref a);

                    // Inclusive end - don't know if it would be safe to have a ref pointing
                    // beyond the value as long as we don't read it
                    ref long end = ref Unsafe.Add(ref p, LongCount - 1);

                    do
                    {
                        if (p != 0) return false;
                        p = ref Unsafe.Add(ref p, 1);
                    } while (!Unsafe.IsAddressGreaterThan(ref p, ref end));
                }

                if (ByteRemainder > 0)
                {
                    ref byte p = ref Unsafe.Add(
                                   ref Unsafe.As<T, byte>(ref a),
                                   ByteCount - ByteRemainder);

                    ref byte end = ref Unsafe.Add(ref p, ByteRemainder - 1);

                    do
                    {
                        if (p != 0) return false;
                        p = ref Unsafe.Add(ref p, 1);
                    } while (!Unsafe.IsAddressGreaterThan(ref p, ref end));
                }

                return true;
            }
        }
    }
}