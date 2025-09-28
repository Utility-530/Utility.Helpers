using System.Diagnostics;
using System.Reactive.Linq;
using Utility.Changes;
using Utility.Interfaces.NonGeneric;

namespace Utility.Extensions
{
    public static class DescriptorHelper
    {
        public static void VisitDescendants(this IChanges tree, Action<IChanges> action)
        {
            action(tree);

            tree.Changes
                .Cast<Change<IDescriptor>>()
                .Subscribe(a =>
                {
                    if (a.Type == Changes.Type.Add && a.Value is IChanges children)
                    {
                        children.VisitDescendants(action);
                        Trace.WriteLine(a.Value.ParentType + " " + a.Value.Type?.Name + " " + a.Value.Name);
                    }
                    else
                    {
                    }
                }, e =>
                {

                });
        }

        public static void VisitChildren(this IChanges tree, Action<object> action)
        {
            tree.Changes
                .Cast<Change<IDescriptor>>()
                .Subscribe(a =>
                {
                    if (a.Type == Changes.Type.Add)
                    {
                        Trace.WriteLine(a.Value.ParentType + " " + a.Value.Type?.Name + " " + a.Value.Name);
                        action(a.Value);
                    }
                    else
                    {
                    }
                }, e =>
                {

                });
        }
    }
}