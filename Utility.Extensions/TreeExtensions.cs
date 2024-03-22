namespace Utility.Extensions
{
    using MoreLinq;
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Utility.Changes;
    using Utility.Interfaces.NonGeneric;
    using Utility.Trees;
    using Utility.Trees.Abstractions;
    using Utility.Reactives;
    using Type = Changes.Type;

    public static partial class TreeExtensions
    {
        ///// <summary> Converts given collection to tree. </summary>
        ///// <typeparam name="T">Custom data type to associate with tree node.</typeparam>
        ///// <param name="items">The collection items.</param>
        ///// <param name="parentSelector">Expression to select parent.</param>
        public static IObservable<ITree<T>> ToTree<T, K>(this IObservable<T> collection, Func<T, K> id_selector, Func<T, K> parent_id_selector, K? root_id = default)
        {
            return Observable.Create<ITree<T>>(observer =>
            {
                CompositeDisposable disposables = new();
                var dis = collection
                            .Where(c => EqualityComparer<K>.Default.Equals(parent_id_selector(c), root_id))
                            .Subscribe(a =>
                            {
                                var tree = new Tree<T>(a);
                                observer.OnNext(tree);
                                var _dis = ToTree(collection, id_selector, parent_id_selector, id_selector(a))
                                     .Subscribe(a =>
                                     {
                                         tree.Add(a);
                                     });
                                disposables.Add(_dis);
                            });
                disposables.Add(dis);
                return disposables;
            });
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

        public static IDisposable ExploreTree<T, TR>(T items, Func<T, TR, T> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property) where TR : IReadOnlyTree, IEquatable
        {
            return ExploreTree(items, funcAdd, funcRemove, funcClear, property, (TR a) => true);
        }

        public static IDisposable ExploreTree<T, TR>(T items, Func<T, TR, T> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Predicate<TR> predicate) where TR : IReadOnlyTree, IEquatable
        {
            return ExploreTree(items, funcAdd, funcRemove, funcClear, property, a => a.Items.Changes<TR>(), predicate);
        }

        public static IDisposable ExploreTree<T, TR>(T items, Func<T, TR, T> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Func<TR, IObservable<Set<TR>>> func, Predicate<TR> predicate) where TR : IEquatable
        {
            if (predicate(property) == false)
                return Disposable.Empty;

            items = funcAdd(items, property);

            var disposable = func(property)
                .Subscribe(args =>
                {
                    foreach (var item in args)
                    {
                        if (item is Change { Type: Type.Add, Value: TR value })
                            _ = ExploreTree(items, funcAdd, funcRemove, funcClear, value, func, predicate);
                        else if (item is Change { Type: Type.Remove, Value: TR _value })
                            funcRemove(items, _value);
                        else if (item is Change { Type: Type.Reset })
                            funcClear(items);
                    }
                },
                e =>
                {
                },
                () =>
                {
                }
              );
            return disposable;
        }
        public static ITree Create(object data)
        {
            return new Tree(data);
        }

        public static ITree Create(object data, params ITree[] items)
        {
            return new Tree(data, items);
        }

        public static ITree Create(object data, params object[] items)
        {
            return new Tree(data, items);
        }

        public static void Visit(this ITree tree, Action<ITree> action)
        {
            action(tree);
            if (tree.HasItems)
                foreach (var item in tree)
                    Visit(item, action);
        }

        public static void VisitAncestors(this ITree tree, Action<ITree> action)
        {
            action(tree);
            if (tree.Parent is ITree parent)
                parent.VisitAncestors(action);
        }


        public static void VisitDescendants(this IReadOnlyTree tree, Action<IReadOnlyTree> action)
        {
            action(tree);

            foreach (var item in tree.Items)
            {
                if (item is IReadOnlyTree t)
                    t.VisitDescendants(action);
            }
        }


        public static IReadOnlyTree? MatchAncestor(this IReadOnlyTree tree, Predicate<IReadOnlyTree> action)
        {
            if (action(tree))
            {
                return tree;
            }

            if (tree.Parent is IReadOnlyTree parent)
            {
                return parent.MatchAncestor(action);
            }


            return null;
        }


        public static IReadOnlyTree? MatchDescendant(this IReadOnlyTree tree, Predicate<IReadOnlyTree> action)
        {
            if (action(tree))
            {
                return tree;
            }
            List<IReadOnlyTree> trees = new();
            var items = tree.Items;
            while (tree is ITree { HasMoreChildren: true })
            {

            }
            foreach (var child in tree.Items)
                if (child is IReadOnlyTree tChild)
                {
                    if (action(tChild))
                    {
                        return tChild;
                    }
                    else
                        trees.Add(tChild);
                }
                else
                    throw new Exception("c 333211");


            foreach (var c in trees)
            {
                if (c.MatchDescendant(action) is { } match)
                    return match;
            }
            return null;
        }

        public static bool IsRoot(this ITree tree)
        {
            return tree.Index.IsEmpty;
        }

        public static IEnumerable<IReadOnlyTree> Ancestors(this IReadOnlyTree tree)
        {
            IReadOnlyTree parent = tree.Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        public static IEnumerable<IReadOnlyTree> MatchAncestors(this IReadOnlyTree tree, Predicate<IReadOnlyTree> predicate)
        {
            if (predicate(tree))
            {
                yield return tree;
            }
            IReadOnlyTree parent = tree.Parent;
            while (parent != null)
            {
                if (predicate(parent))
                    yield return parent;
                parent = parent.Parent;
            }
        }



    }
}