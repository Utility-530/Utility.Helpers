using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UtilityHelper
{
    public static class ReflectionHelper
    {
        public static IEnumerable<string> SelectPropertyNamesOfDeclaringType<T>()

           => typeof(T).GetProperties()
              .Where(x => x.DeclaringType == typeof(T))
              .Select(info => info.Name);

        public static bool IsReadOnly(this PropertyInfo prop)
        {
            ReadOnlyAttribute? attrib = Attribute.GetCustomAttribute(prop, typeof(ReadOnlyAttribute)) as ReadOnlyAttribute;
            bool ro = !prop.CanWrite || (attrib != null && attrib.IsReadOnly);
            return ro;
        }

        public static IEnumerable<T?> TypesOf<T>(this IEnumerable<Assembly> assemblies) where T : class
        {
            return from type in assemblies.AllTypes()
                   where typeof(T).IsAssignableFrom(type) && !type.IsAbstract
                   select Activator.CreateInstance(type) as T;
        }

        public static IEnumerable<TypeInfo> AllTypes(this IEnumerable<Assembly> assembliesToScan)
        {
            return assembliesToScan
                .SelectMany(a => a.DefinedTypes);
        }

        public static IEnumerable RecursivePropertyValues(object e, string path)
        {
            List<IEnumerable> lst = new List<IEnumerable>();
            lst.Add(new[] { e });
            try
            {
                var xx = PropertyHelper.GetPropertyRefValue<IEnumerable>(e, path);
                foreach (var x in xx)
                    lst.Add(RecursivePropertyValues(x, path));
            }
            catch (Exception ex)
            {
                //
            }
            return lst.SelectMany(a => a.Cast<object>());
        }

        public static IEnumerable<(string, Func<object?>)> GetStaticMethods(this Type t, params object[] parameters)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(null, parameters))));
        }

        public static IEnumerable<(string, Func<object?>)> GetMethods(this object instance, params object[] parameters)
        {
            return instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(instance, parameters))));
        }

        public static string GetDescription(this MethodInfo methodInfo)
        {
            try
            {
                object[] attribArray = methodInfo.GetCustomAttributes(false);

                if (attribArray.Length > 0)
                {
                    if (attribArray[0] is DescriptionAttribute attrib)
                        return attrib.Description;
                }
            }
            catch (NullReferenceException)
            {
            }
            return methodInfo.Name;
        }

        ///<summary>
        /// <a href="https://dotnetcoretutorials.com/2020/07/03/getting-assemblies-is-harder-than-you-think-in-c/"></a>
        /// </summary>
        public static IEnumerable<Assembly> GetAssemblies(Predicate<AssemblyName>? predicate = null)
        {
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            if (Assembly.GetEntryAssembly() is { } ass)
            {
                assembliesToCheck.Enqueue(ass);
            }

            while (assembliesToCheck.Any())
            {
                foreach (var (reference, assembly) in from reference in assembliesToCheck.Dequeue().GetReferencedAssemblies()
                                                      where (predicate?.Invoke(reference) ?? true) && loadedAssemblies.Contains(reference.FullName) == false
                                                      let assembly = Assembly.Load(reference)
                                                      select (reference, assembly))
                {
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    yield return assembly;
                }
            }
        }

        public static IEnumerable<Assembly> GetSolutionAssemblies()
        {
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                                .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
            return assemblies;
        }
    }
}