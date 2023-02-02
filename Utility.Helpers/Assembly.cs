using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utility.Helpers
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> GetTypesInNamespace(this Assembly assembly, string nameSpace) => from t in assembly.GetTypes()
                                                                                                         where String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
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
                                                                                             && !String.IsNullOrEmpty(assembly.Location);

        public static bool FullNameCheck(string assemblyFullName) =>
    !assemblyFullName.StartsWith("System")
    && !assemblyFullName.StartsWith("Microsoft")
    && !assemblyFullName.StartsWith("netstandard")
    && assemblyFullName.IndexOf("CppCodeProvider") == -1
    && assemblyFullName.IndexOf("WebMatrix") == -1
    && assemblyFullName.IndexOf("SMDiagnostics") == -1;

        public static bool LocationCheck(string assemblyLocation) => !String.IsNullOrEmpty(assemblyLocation) &&
            assemblyLocation.IndexOf("App_Web") == -1 &&
            assemblyLocation.IndexOf("App_global") == -1;

        public static bool ManifestModuleCheck(string assemblyManifestModuleName) => assemblyManifestModuleName != "<In Memory Module>";
    }
}