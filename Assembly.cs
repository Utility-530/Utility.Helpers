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
    public static class AssemblyHelper
    {

        public static IEnumerable<Assembly> GetSolutionAssemblies()
        {
            var list = new HashSet<string>();
            var stack = new Stack<Assembly>();

            stack.Push(Assembly.GetEntryAssembly());

            while (stack.Count > 0)
            {
                var asm = stack.Pop();

                yield return asm;

                foreach (var reference in asm.GetReferencedAssemblies())
                    if (!reference.FullName.Contains("Deedle"))
                        if (list.Add(reference.FullName))
                            stack.Push(Assembly.Load(reference));



            }


        }

    }
}
