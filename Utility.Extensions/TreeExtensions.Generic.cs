using Utility.Helpers.Generic;
using Utility.Helpers.NonGeneric;
using Utility.Trees;
using Utility.Trees.Abstractions;

namespace Utility.Extensions
{
    public static class TreeExtensions_Generic
    {
        public static IEnumerable<ITree<T>> ToTree<T, K>(this IEnumerable<T> collection, Func<T, K> id_selector, Func<T, K> parent_id_selector, K? root_id = default)
        {
            return ToTree(collection, id_selector, parent_id_selector, a => (ITree<T>)new Tree<T>(a), root_id);
        }

        public static IEnumerable<TTree> ToTree<T, K, TTree>(this IEnumerable<T> collection, Func<T, K> id_selector, Func<T, K> parent_id_selector, Func<T, TTree> conversion, K? root_id = default) where TTree : Utility.Interfaces.Generic.IAdd<TTree>
        {

            foreach (var item in collection.Where(c => EqualityComparer<K>.Default.Equals(parent_id_selector(c), root_id)))
            {
                var tree = conversion(item);
                yield return tree;
                ToTree(collection, id_selector, parent_id_selector, conversion, id_selector(item)).ForEach(tree.Add);
            }
        }
        public static void Visit<T>(this T tree, Func<T, IEnumerable<T>> children, Action<T> action)
        {
            action(tree);
            foreach (var item in children(tree))
                Visit(item, children, action);
        }

        public static bool IsRoot(this IReadOnlyTree tree)  => tree.Parent == null;

        public static bool IsLeaf<T>(this ITree<T> tree) => tree.Count() == 0;

        //public static int Level<T>(this ITree<T> tree) => tree.IsRoot() ? 0 : tree.Parent.Level() + 1;

        public static void Add<T>(this ITree<T> tree, T data)
        {
        }

        public static void Remove<T>(this ITree<T> tree, T data)
        {
        }

        public static ITree<T> Create<T>(T data)
        {
            return new Tree<T>(data);
        }

        public static ITree<T> Create<T>(T data, params ITree<T>[] items)
        {
            return new Tree<T>(data, items);
        }

        public static ITree<T> Create<T>(T data, params object[] items)
        {
            return new Tree<T>(data, items);
        }

        public static void Visit<T>(this ITree<T> tree, Action<ITree<T>> action)
        {
            action(tree);
            if (tree.HasItems)
                foreach (var item in tree as IEnumerable<ITree<T>>)
                    Visit(item, action);
        }

        public static ITree<T>? Match<T>(this ITree<T> tree, Predicate<ITree<T>> action)
        {
            if (action(tree))
            {
                return tree;
            }
            else if (tree.HasItems)
            {
                foreach (var item in tree as IEnumerable<ITree<T>>)
                {
                    if (Match(item, action) is ITree<T> sth)
                    {
                        return sth;
                    }
                }
            }

            return null;
        }

        public static ITree<T>? Match<T>(this ITree<T> tree, T data)
        {
            return Match(tree, a => a.Data?.Equals(data) == true);
        }

        public static ITree<T>? Match<T>(this ITree<T> tree, Guid guid)
        {
            return Match(tree, a => a.Key.Equals(guid));
        }
    }
}