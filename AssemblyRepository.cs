using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UtilityHelper
{
    public class AssemblyRepository
    {
        private Lazy<InternalAssemblyRepository> lazyRepo;

        public AssemblyRepository(Assembly assembly)
        {
            lazyRepo = new Lazy<InternalAssemblyRepository>(() => new InternalAssemblyRepository(assembly));
        }

        public Dictionary<string, Assembly> DependentAssemblyList => lazyRepo.Value._dependentAssemblyList;

        public List<MissingAssembly> MissingAssemblyList => lazyRepo.Value._missingAssemblyList;

        public readonly struct MissingAssembly
        {
            public MissingAssembly(string missingAssemblyName, string missingAssemblyNameParent)
            {
                MissingAssemblyName = missingAssemblyName;
                MissingAssemblyNameParent = missingAssemblyNameParent;
            }

            public string MissingAssemblyName { get; }
            public string MissingAssemblyNameParent { get; }
        }

        public class InternalAssemblyRepository
        {
            public readonly Dictionary<string, Assembly> _dependentAssemblyList = new Dictionary<string, Assembly>();
            public readonly List<MissingAssembly> _missingAssemblyList = new List<MissingAssembly>();

            public InternalAssemblyRepository(Assembly assembly)
            {
                _dependentAssemblyList = new Dictionary<string, Assembly>();
                _missingAssemblyList = new List<MissingAssembly>();

                InternalFindDependentAssembliesRecursive(assembly, ref _missingAssemblyList, ref _dependentAssemblyList);

                // Only include assemblies that we wrote ourselves (ignore ones from GAC).
                foreach (var k in _dependentAssemblyList.Values.Where(o => o.GlobalAssemblyCache == true).ToList())
                {
                    _dependentAssemblyList.Remove(k.FullName.MyToName());
                }
            }

            /// <summary>
            ///     Intent: Internal recursive class to get all dependent assemblies, and all dependent assemblies of
            ///     dependent assemblies, etc.
            /// </summary>
            private static void InternalFindDependentAssembliesRecursive(Assembly assembly, ref List<MissingAssembly> missingAssemblies, ref Dictionary<string, Assembly> dependentAssemblyList)
            {
                // Load assemblies with newest versions first. Omitting the ordering results in false positives on
                // _missingAssemblyList.
                foreach (var r in from ass in assembly.GetReferencedAssemblies()
                                  where string.IsNullOrEmpty(ass.FullName) == false
                                  orderby ass.Version
                                  select ass)
                {
                    if (dependentAssemblyList.ContainsKey(r.FullName.MyToName()) == false)
                    {
                        try
                        {
                            var a = Assembly.ReflectionOnlyLoad(r.FullName);
                            dependentAssemblyList[a.FullName.MyToName()] = a;
                            InternalFindDependentAssembliesRecursive(a, ref missingAssemblies, ref dependentAssemblyList);
                        }
                        catch (Exception ex)
                        {
                            missingAssemblies.Add(new MissingAssembly(r.FullName.Split(',')[0], assembly.FullName.MyToName()));
                        }
                    }
                }
            }
        }
    }

    internal static class Helper
    {
        public static string MyToName(this string fullName)
        {
            return fullName.Split(',')[0];
        }
    }
}