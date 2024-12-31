using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utility.Helpers
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

        public static IEnumerable<(string, MethodInfo)> StaticMethods(this Type t)
        {
            return t
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Select(m => (m.GetDescription(), m));
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



        public static string AsString(this MethodInfo mi)
        {
            StringBuilder sb = new();
            // Get method body information.
            //MethodInfo mi = typeof(Example).GetMethod("MethodBodyExample");
            MethodBody mb = mi.GetMethodBody();
            sb.AppendLine($"Method: {mi}");

            // Display the general information included in the
            // MethodBody object.
            sb.AppendLine($"Local variables are initialized: {mb.InitLocals}");
            sb.AppendLine($"Maximum number of items on the operand stack: {mb.MaxStackSize}");

            // Display information about the local variables in the
            // method body.
            sb.AppendLine();
            foreach (LocalVariableInfo lvi in mb.LocalVariables)
            {
                sb.AppendLine($"Local variable: {lvi}");
            }

            // Display exception handling clauses.
            sb.AppendLine();
            foreach (ExceptionHandlingClause ehc in mb.ExceptionHandlingClauses)
            {
                sb.AppendLine(ehc.Flags.ToString());

                // The FilterOffset property is meaningful only for Filter
                // clauses. The CatchType property is not meaningful for
                // Filter or Finally clauses.
                switch (ehc.Flags)
                {
                    case ExceptionHandlingClauseOptions.Filter:
                        sb.AppendLine($"Filter Offset: {ehc.FilterOffset}"
                            );
                        break;

                    case ExceptionHandlingClauseOptions.Finally:
                        break;

                    default:
                        sb.AppendLine($"Type of exception: {ehc.CatchType}");
                        break;
                }

                sb.AppendLine($"Handler Length: {ehc.HandlerLength}");
                sb.AppendLine($"Handler Offset: {ehc.HandlerOffset}");
                sb.AppendLine($"Try Block Length: {ehc.TryLength}");
                sb.AppendLine($"Try Block Offset: {ehc.TryOffset}");
            }
            return sb.ToString();
        }

        // This code example produces output similar to the following:
        //
        //Method: Void MethodBodyExample(System.Object)
        //    Local variables are initialized: True
        //    Maximum number of items on the operand stack: 2
        //
        //Local variable: System.Int32 (0)
        //Local variable: System.String (1)
        //Local variable: System.Exception (2)
        //Local variable: System.Boolean (3)
        //
        //Filter
        //      Filter Offset: 71
        //      Handler Length: 23
        //      Handler Offset: 116
        //      Try Block Length: 61
        //      Try Block Offset: 10
        //Clause
        //    Type of exception: System.Exception
        //       Handler Length: 21
        //       Handler Offset: 70
        //     Try Block Length: 61
        //     Try Block Offset: 9
        //Finally
        //       Handler Length: 14
        //       Handler Offset: 94
        //     Try Block Length: 85
        //     Try Block Offset: 9
    }
}
