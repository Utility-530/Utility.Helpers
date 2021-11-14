using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Utility
{


    public static class TypeHelper
    {

        public static bool IsDerivedFrom<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        public static IEnumerable<Type> GetTypesFromTheSameAssembly(this IEnumerable<Type> types, Predicate<Type>? predicate = default)
        {
            return types.SelectMany(a => a.Assembly.GetTypes()).Where(a => predicate?.Invoke(a) ?? true);
        }

        public static bool IsNumericType(this Type o)
        {
            return Type.GetTypeCode(o) switch
            {
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or
                TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
                _ => false,
            };
        }

        /// <summary>
        /// Checks whether all types in an enumerable are the same.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool OfSameType(this IEnumerable enumerable) => OfSameType(enumerable, out Type _);

        /// <summary>
        /// Checks whether all types in an enumerable are the same.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool OfSameType(this IEnumerable enumerable, out Type type)
        {
            var (t, sameType) =
                enumerable
                    .Cast<object>()
                    .Select(a => a.GetType())
                    .AggregateUntil(
                        (type: default(Type), sameType: true),
                        (a, b) => (b, a.type == null || a.type == b),
                        a => !a.sameType);
            type = t!;
            return sameType;
        }


        public static bool OfClassType(this IEnumerable enumerable) =>
            enumerable
                .Cast<object>()
                .Select(a => a.GetType())
                .All(a => a.IsClass);


        public static bool NotOfClassType(this IEnumerable enumerable) =>
            enumerable
                .Cast<object>()
                .Select(a => a.GetType())
                .All(a => a.IsClass == false);

        public static bool IsDerivedFromGenericType(this Type type, Type interfaceType)
        {
           return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}