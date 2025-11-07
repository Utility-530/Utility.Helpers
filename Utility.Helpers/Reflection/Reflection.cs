using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Utility.Helpers.Reflection
{
    public static class ReflectionHelper
    {
        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? GetField(type.BaseType ?? throw new Exception("ubn 43"), name);
        }

        public static void SetFieldValue(this object instance, string name, object value)
        {
            GetField(instance.GetType(), name).SetValue(instance, value);
        }

        public static void SetFieldValue(this object instance, string name, object value, ref FieldInfo fieldInfo)
        {
            (fieldInfo ??= GetField(instance.GetType(), name)).SetValue(instance, value);
        }

        #region Obsolete

        [Obsolete]
        public static IEnumerable<(string, Func<object?>)> GetStaticMethods(this Type t, params object[] parameters)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(null, parameters))));
        }

        //[Obsolete]
        //public static IEnumerable<(string, MethodInfo)> StaticMethods(this Type t)
        //{
        //    return t
        //            .GetMethods(BindingFlags.Public | BindingFlags.Static)
        //                .Select(m => (m.GetDescription(), m));
        //}

        [Obsolete]
        public static IEnumerable<(string, Func<object?>)> GetMethods(this object instance, params object[] parameters)
        {
            return instance.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(m => (m.GetDescription(), new Func<object?>(() => m.Invoke(instance, parameters))));
        }

        [Obsolete]
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

        #endregion Obsolete

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