namespace Utility.Extensions
{
    using MoreLinq;
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Utility.Changes;
    using Utility.Helpers.NonGeneric;
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

        public static void Visit<T>(this ITree<T> tree, Action<ITree<T>> action)
        {
            action(tree);
            foreach (var item in (tree as IEnumerable<ITree<T>>))
                Visit(item, action);
        }

        public static void Visit<T>(this T tree, Func<T, IEnumerable<T>> children, Action<T> action)
        {
            action(tree);
            foreach (var item in children(tree))
                Visit(item, children, action);
        }

        public static bool IsRoot<T>(this ITree<T> tree) => tree.Parent == null;

        public static bool IsLeaf<T>(this ITree<T> tree) => tree.Count() == 0;

        public static int Level<T>(this ITree<T> tree) => tree.IsRoot() ? 0 : tree.Parent.Level() + 1;
    }
}