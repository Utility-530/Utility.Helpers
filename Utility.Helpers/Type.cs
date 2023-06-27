using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Utility.Helpers
{
    public static class TypeHelper
    {
        public static Type[] GenericTypeArguments(this Type? type)
        {
            if (type == null)
                return Array.Empty<Type>();

            if (type?.IsGenericType == true)
            {
                return type.GetGenericArguments();
            }
            else
                return GenericTypeArguments(type?.BaseType);

        }

        public static string AsString(string assemblyName, string nameSpace, string name)
        {
            return nameSpace + "." + name + ", " + assemblyName;
        }
        public static Type ToType(string assemblyName, string nameSpace, string name)
        {
            return Type.GetType(AsString(assemblyName, nameSpace, name));
        }

        public static string AsString(this Type type)
        {
            return AsString(type.Assembly.ToString(), type.Namespace, type.Name);
        }

        public static (string? assembly, string? @namespace, string name) AsTuple(this Type type)
        {
            return (type.Assembly.FullName, type.Namespace, type.Name);
        }

        public static T? OfType<T>(object value, Type? type = null)
        {
            type ??= typeof(object);
            if (value.GetType().GetGenericArguments()[0].IsAssignableFrom(type))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// <a href="https://stackoverflow.com/questions/1900353/how-to-get-the-type-contained-in-a-collection-through-reflection"></a>
        /// </summary>
        /// <param name="collectionType"></param>
        /// <returns>the inner type of a collection type e.g IEnumerable</returns>
        public static Type InnerType(this Type collectionType)
        {
            Type? ienum = FindIEnumerable(collectionType);
            if (ienum == null) return collectionType;
            return ienum.GetGenericArguments()[0];

            static Type? FindIEnumerable(Type seqType)
            {
                if (seqType == null || seqType == typeof(string))
                    return null;
                if (seqType.IsArray)
                    return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
                if (seqType.IsGenericType)
                {
                    foreach (Type arg in seqType.GetGenericArguments())
                    {
                        Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                        if (ienum.IsAssignableFrom(seqType))
                        {
                            return ienum;
                        }
                    }
                }
                Type[] ifaces = seqType.GetInterfaces();
                if (ifaces != null && ifaces.Length > 0)
                {
                    foreach (Type iface in ifaces)
                    {
                        Type? ienum = FindIEnumerable(iface);
                        if (ienum != null)
                            return ienum;
                    }
                }
                if (seqType.BaseType != null && seqType.BaseType != typeof(object))
                {
                    return FindIEnumerable(seqType.BaseType);
                }
                return null;
            }
        }


        public static IEnumerable<Type> Filter<T>() => Filter(typeof(T));

        public static IEnumerable<Type> Filter(Type type) =>

        from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
        from assemblyType in domainAssembly.GetTypes()
        where type.IsAssignableFrom(assemblyType) && type != (assemblyType)
        select assemblyType;

        public static string GetDescription<T>()
        {
            return GetDescription(typeof(T));
        }

        public static string GetDescription(this Type type)
        {
            return type.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().Single().Description;
        }

        //Stack Overflow nawfal Oct 9 '13 at 7:44
        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type.GetMethods()
                 .FirstOrDefault(m =>
                 m.Name == name &&
                 m.GetParameters()
                 .Select(p => p.ParameterType)
                 .SequenceEqual(parameterTypes, new SimpleTypeComparer()));
        }

        //Stack Overflow Dustin Campbell answered Oct 27 '10 at 17:58
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<KeyValuePair<string, Func<T>>> LoadMethods<T>(this Type t, params object[] parameters)
        {
            return Assembly.GetAssembly(t)
                  .GetType(t.FullName)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                       // filter by return type
                       .Where(a => a.ReturnType.Name == typeof(T).Name)
                        .Select(a => new KeyValuePair<string, Func<T>>(a.Name, () => (T)a.Invoke(null, parameters)));
        }

        public static IEnumerable<KeyValuePair<string, Action>> ToActions<T>(this IEnumerable<KeyValuePair<string, Func<T>>> kvps, Action<T> tr)
        {
            foreach (var m in kvps)
            {
                yield return new KeyValuePair<string, Action>(
                      m.Key,
                    () => tr(m.Value())

                );
            }
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute(Type @class, Type attribute)

        => @class.GetProperties()
        .Where(prop => Attribute.IsDefined(prop, attribute));

        public static IEnumerable<MethodInfo> GetMethodsByAttribute(Type t, Type attribute) =>
            t.GetMethods().Where(
                    method => Attribute.IsDefined(method, attribute));

        public static Func<T, R> GetInstanceMethod<T, R>(MethodInfo method)
        {
            //ParameterExpression x = Expression.Parameter(typeof(T), "it");
            return Expression.Lambda<Func<T, R>>(
                Expression.Call(null, method), null).Compile();
        }

        public static IEnumerable<Type> GetInheritingTypes(Type type)
        {
            var x = AssemblyHelper.GetNonSystemAssemblies();

            var sf = x.SelectMany(sd => sd.GetExportedTypes());

            var v = sf.Where(p => p.GetInterfaces().Any(t => t == type));

            return v;
        }

        public static Type[] GetTypesByAssembly<T>() => typeof(T).GetTypesByAssembly();

        public static Type[] GetTypesByAssembly(this Type t) => t.Assembly.GetTypes();

        public static bool IsNullableType(System.Type type) => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

        public static IEnumerable<KeyValuePair<string, Type>> ToKeyValuePairs(IEnumerable<Type> types) => types.Select(a => new KeyValuePair<string, Type>(a.ToString(), a));

        public static bool IsNumericType(this Type o)
        {
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;

                default:
                    return false;
            }
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


        public static Type GetElementType(this Type collectionType)
        {
            if (collectionType == null)
            {
                throw new ArgumentNullException("collectionType");
            }

            foreach (Type iface in collectionType.GetInterfaces())
            {
                if (!iface.IsGenericType)
                {
                    continue;
                }

                if (iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return iface.GetGenericArguments()[1];
                }

                if (iface.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return iface.GetGenericArguments()[0];
                }

                if (iface.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    return iface.GetGenericArguments()[0];
                }

                if (iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return iface.GetGenericArguments()[0];
                }
            }
            return typeof(object);
        }

        public static int GetEnumMaxPower(this Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }

            if (!enumType.IsEnum)
            {
                throw new ArgumentException(null, "enumType");
            }

            Type utype = Enum.GetUnderlyingType(enumType);
            return GetEnumUnderlyingTypeMaxPower(utype);
        }

        public static int GetEnumUnderlyingTypeMaxPower(this Type underlyingType)
        {
            if (underlyingType == null)
            {
                throw new ArgumentNullException("underlyingType");
            }

            if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
            {
                return 64;
            }

            if (underlyingType == typeof(int) || underlyingType == typeof(uint))
            {
                return 32;
            }

            if (underlyingType == typeof(short) || underlyingType == typeof(ushort))
            {
                return 16;
            }

            if (underlyingType == typeof(byte) || underlyingType == typeof(sbyte))
            {
                return 8;
            }

            throw new ArgumentException(null, "underlyingType");
        }

        public static bool IsFlagsEnum(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!type.IsEnum)
            {
                return false;
            }

            return type.IsDefined(typeof(FlagsAttribute), true);
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}