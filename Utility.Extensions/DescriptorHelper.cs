using System.Diagnostics;
using System.Reactive.Linq;
using Utility.Changes;
using Utility.Interfaces.NonGeneric;

namespace Utility.Extensions
{
    public static class DescriptorHelper
    {
        public static void VisitDescendants(this IChildren tree, Action<IChildren> action)
        {
            action(tree);

            tree.Children
                .Cast<Change<IDescriptor>>()
                .Subscribe(a =>
                {
                    if (a.Type == Changes.Type.Add && a.Value is IChildren children)
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

        public static void VisitChildren(this IChildren tree, Action<object> action)
        {
            tree.Children
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

        public static T Get<T>(this IGet get)
        {
            return (T)get.Get();
        }

        public static void Set<T>(this ISet get, T t)
        {
            get.Set(t);
        }
    }
}