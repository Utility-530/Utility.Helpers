using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Utility.Helpers;

namespace Utility.Helpers.Reflection
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> TypesInNamespace(this Assembly assembly, string nameSpace) => from t in assembly.GetTypes()
                                                                                                      where string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
                                                                                                      select t;

        public static IEnumerable<KeyValuePair<string, object>> CreateNonSystemTypesByInterface(params Type[] interfaceTypes) => CreateTypesByInterface(GetNonSystemAssemblies().ToArray(), interfaceTypes);

        public static IEnumerable<KeyValuePair<string, object>> CreateTypesByInterface(Assembly[] assemblies, params Type[] interfaceTypes) =>
                                              from x in assemblies.SelectMany(s => s.GetTypes().Select(t => new { s.GetName().Name, t }))
                                              where interfaceTypes.Any(interfaceType =>
                                              // assignable from interface
                                              interfaceType.IsAssignableFrom(x.t) &&
                                              x.t.FullName != interfaceType.FullName) &&
                                              // not abstract
                                              !x.t.IsAbstract &&
                                              // has parameterless constructor
                                              x.t.GetConstructor(Type.EmptyTypes) != null
                                              select new KeyValuePair<string, object>(x.Name, Activator.CreateInstance(x.t));

        public static IEnumerable<Assembly> GetNonSystemAssemblies() => from assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                                                                        where FullNameCheck(assemblyName.FullName)
                                                                        let assembly = Assembly.Load(assemblyName.Name)
                                                                        where LocationCheck(assembly.Location) && ManifestModuleCheck(assembly.ManifestModule.Name)
                                                                        select assembly;

        public static IEnumerable<Assembly> GetNonSystemAssembliesInCurrentDomain() => FilterNonSystemAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        public static IEnumerable<Assembly> FilterNonSystemAssemblies(Assembly[] assemblies) => from assembly in assemblies
                                                                                                where assembly.FullCheck()
                                                                                                select assembly;

        public static bool FullCheck(this Assembly assembly) => assembly.ManifestModule.Name != "<In Memory Module>"
                                                                                             && !assembly.FullName.StartsWith("System")
                                                                                             && !assembly.FullName.StartsWith("Microsoft")
                                                                                             && assembly.Location.IndexOf("App_Web") == -1
                                                                                             && assembly.Location.IndexOf("App_global") == -1
                                                                                             && assembly.FullName.IndexOf("CppCodeProvider") == -1
                                                                                             && assembly.FullName.IndexOf("WebMatrix") == -1
                                                                                             && assembly.FullName.IndexOf("SMDiagnostics") == -1
                                                                                             && !string.IsNullOrEmpty(assembly.Location);

        public static bool FullNameCheck(string assemblyFullName) =>
    !assemblyFullName.StartsWith("System")
    && !assemblyFullName.StartsWith("Microsoft")
    && !assemblyFullName.StartsWith("netstandard")
    && assemblyFullName.IndexOf("CppCodeProvider") == -1
    && assemblyFullName.IndexOf("WebMatrix") == -1
    && assemblyFullName.IndexOf("SMDiagnostics") == -1;

        public static bool LocationCheck(string assemblyLocation) => !string.IsNullOrEmpty(assemblyLocation) &&
            assemblyLocation.IndexOf("App_Web") == -1 &&
            assemblyLocation.IndexOf("App_global") == -1;

        public static bool ManifestModuleCheck(string assemblyManifestModuleName) => assemblyManifestModuleName != "<In Memory Module>";

        public static Assembly ToAssembly(this string typeSerialised)
        {
            return Assembly.LoadFrom(Regex.Match(typeSerialised, TypeHelper.myRegex).Groups[2].Value);
        }

        public static IEnumerable<TypeInfo> AllTypes(this IEnumerable<Assembly> assembliesToScan)
        {
            return assembliesToScan
                .SelectMany(a => a.DefinedTypes);
        }

        public static IEnumerable<T?> TypesOf<T>(this IEnumerable<Assembly> assemblies) where T : class
        {
            return from type in assemblies.AllTypes()
                   where typeof(T).IsAssignableFrom(type) && !type.IsAbstract
                   select Activator.CreateInstance(type) as T;
        }

        public static IEnumerable<Type> TypesByAttribute<TA>(this Assembly assembly, Func<TA, int> orderBy) where TA : Attribute =>
                from t in assembly.GetTypes()
                let x = t.GetCustomAttribute<TA>()
                where x != null
                orderby orderBy(x)
                select t;

        public static IEnumerable<(Type type, TA attribute)> TypesAndAttributeByAttribute<TA>(this Assembly assembly, Func<TA, int> orderBy) where TA : Attribute =>
             from t in assembly.GetTypes()
             let x = t.GetCustomAttribute<TA>()
             where x != null
             orderby orderBy(x)
             select (t, x);
    }
}