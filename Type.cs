using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilityHelper.NonGeneric;


namespace UtilityHelper
{


    public static class TypeHelper
    {



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

    }
}
