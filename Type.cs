using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UtilityHelper
{
    public static class TypeHelper
    {
        public static Type ToType(string assemblyName, string nameSpace, string name)
        {
            Type your = Type.GetType(nameSpace + "." + name + ", " + assemblyName);
            return your;
        }
        public static string[] AsString(this Type type)
        {
            return new[] { type.Assembly.FullName, type.Namespace, type.Name };
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
                        .Select(_ => new KeyValuePair<string, Func<T>>(_.Name, () => (T)_.Invoke(null, parameters)));
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

        public static IEnumerable<KeyValuePair<string, Type>> ToKeyValuePairs(IEnumerable<Type> types) => types.Select(_ => new KeyValuePair<string, Type>(_.ToString(), _));


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
    }
}