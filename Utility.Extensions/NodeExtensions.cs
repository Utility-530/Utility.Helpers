using System.Reflection;
using Utility.Trees.Abstractions;
using Utility.Nodes;
using Utility.Models.Trees;
using Utility.Reactives;
using Utility.Trees;
using System.Reactive.Disposables;
using Utility.Interfaces.NonGeneric;
using Utility.PropertyNotifications;
using Utility.Interfaces.Exs;

namespace Utility.Extensions
{
    public static class NodeExtensions
    {
        public static ITree ToViewModelTree(this Assembly[] assemblies, Predicate<Type>? typePredicate = null)
        {
            ViewModelTree t_tree = new("root");

            foreach (var assembly in assemblies)
            {
                ViewModelTree tree = new(new AssemblyModel { Name = assembly.GetName().Name, Assembly = assembly });

                foreach (var type in assembly.GetTypes())
                {
                    if (typePredicate?.Invoke(type) == false)
                        continue;
                    var _tree = new ViewModelTree(new TypeModel { Name = type.Name, Type = type })
                    {
                        Parent = tree
                    };
                    tree.Add(_tree);
                }
                t_tree.Add(tree);
            }
            return t_tree;
        }

        /// <summary>
        /// Creates a basic copy of <see cref="tree"/> with commands of the copy linked to the original
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static INode Abstract(this INode tree)
        {
            var _name = (tree.Data is IName { Name: { } name }) ? name : tree.Data.ToString();          
            var clone = new Node(new Abstract { Name = _name }) { Key = tree.Key, AddCommand = tree.AddCommand, RemoveCommand = tree.RemoveCommand, Removed = tree.Removed };
            tree.WithChangesTo(a => a.Removed).Subscribe(a => clone.Removed = a);

            CompositeDisposable disposables = new();
            tree.AndAdditions<Node>().Subscribe(async item =>
            {
                var childClone = (ITree)(item.Abstract());
                childClone.Parent = clone;
                clone.Add(childClone);
            });
            return clone;
        }
    }

    public class Abstract
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }


    }
}
