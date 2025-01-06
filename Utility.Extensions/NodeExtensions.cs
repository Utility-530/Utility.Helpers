using System.Reflection;
using Utility.Trees.Abstractions;
using Utility.Nodes;

namespace Utility.Extensions
{
    public static class NodeExtensions
    {
        public static ITree ToTree(Assembly[] assemblies, Predicate<Type>? typePredicate = null)
        {
            ViewModelTree t_tree = new("root", "root");

            foreach (var assembly in assemblies)
            {
                ViewModelTree tree = new(assembly.GetName().Name, assembly);

                foreach (var type in assembly.GetTypes())
                {
                    if (typePredicate?.Invoke(type) == false)
                        continue;
                    var _tree = new ViewModelTree(type.Name, type)
                    {
                        Parent = tree
                    };
                    tree.Add(_tree);
                }
                t_tree.Add(tree);
            }
            return t_tree;
        }    
    }
}
