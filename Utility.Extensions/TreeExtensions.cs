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
        public static IObservable<ITree<T>> ToTree<T, K>(this IObservable<T> collection, Func<T, K> id_selector, Func<T, K> parent_id_selector, K? root_id = default, Func<T, ITree<T>>? func = null)
        {
            return Observable.Create<ITree<T>>(observer =>
            {
                CompositeDisposable disposables = new();
                var dis = collection
                            .Where(c => EqualityComparer<K>.Default.Equals(parent_id_selector(c), root_id))
                            .Subscribe(a =>
                            {
                                var tree = func?.Invoke(a) ?? new Tree<T>(a);
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

        public static IDisposable ExploreTree<T, TR>(T items, Func<T, TR, T> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Predicate<TR>? predicate = default) where TR : IItems
        {
            return ExploreTree(items, funcAdd, funcRemove, funcClear, property, a => a.Items.AndChanges<TR>(), predicate ??= (TR a) => true);
        }

        public static IDisposable ExploreTree<T, TR, TS>(TS item, Func<TS, T> funcItems, Func<T, TR, TS, TS> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Predicate<TR>? predicate = default) where TR : IItems
        {
            return ExploreTree(item, funcItems, funcAdd, funcRemove, funcClear, property, a => a.Items.AndChanges<TR>(), predicate ??= (TR a) => true);
        }

        public static IDisposable ExploreTree<T, TR>(T items, Func<T, TR, T> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Func<TR, IObservable<Changes.Set<TR>>> func, Predicate<TR>? funcPredicate = null)
        {
            if (funcPredicate?.Invoke(property) == false)
                return Disposable.Empty;

            items = funcAdd(items, property);

            var disposable = func(property)
                .Subscribe(args =>
                {
                    foreach (var item in args)
                    {
                        if (item is Change { Type: Type.Add, Value: TR value })
                            _ = ExploreTree(items, funcAdd, funcRemove, funcClear, value, func, funcPredicate);
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

        public static IDisposable ExploreTree<T, TR, TS>(TS item, Func<TS, T> funcItems, Func<T, TR, TS, TS> funcAdd, Action<T, TR> funcRemove, Action<T> funcClear, TR property, Func<TR, IObservable<Changes.Set<TR>>> func, Predicate<TR>? funcPredicate = null)
        {
            if (funcPredicate?.Invoke(property) == false)
                return Disposable.Empty;

            var items = funcItems(item);
            item = funcAdd(items, property, item);

            var disposable = func(property)
                .Subscribe(args =>
                {
                    foreach (var _item in args)
                    {
                        if (_item is Change { Type: Type.Add, Value: TR value })
                            _ = ExploreTree(item, funcItems, funcAdd, funcRemove, funcClear, value, func, funcPredicate);
                        else if (_item is Change { Type: Type.Remove, Value: TR _value })
                            funcRemove(funcItems(item), _value);
                        else if (_item is Change { Type: Type.Reset })
                            funcClear(funcItems(item));
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


        public static int IndexOf(this IReadOnlyTree tree, IReadOnlyTree _item)
        {
            int i = 0;
            foreach (var item in tree.Items)
            {
                if (item.Equals(_item))
                    return i;
                i++;
            }
            return -1;
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

        public static void Visit<T>(this T tree, Func<T, IEnumerable<T>> children, Action<T> action)
        {
            action(tree);
            foreach (var item in children(tree))
                Visit(item, children, action);
        }

        public static void VisitAncestors(this IReadOnlyTree tree, Action<IReadOnlyTree> action)
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


        public static IObservable<IReadOnlyTree?> MatchDescendant(this ObservableTree tree, Predicate<IReadOnlyTree> action)
        {
            return Observable.Create<IReadOnlyTree?>(observer =>
            {
                if (action(tree))
                {
                    observer.OnNext(tree);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }
                List<IReadOnlyTree> trees = new();
                var items = tree.Items;

                return tree.Subscribe(a =>
                {

                    if (a.Type == Type.Add)
                    {

                        if (a.Value is IReadOnlyTree tChild)
                        {
                            if (action(tChild))
                            {
                                observer.OnNext(tChild);
                                return;
                            }
                            //else
                            //    trees.Add(tChild);
                        }
                        else
                            throw new Exception("c 333211");

                        if (tChild is ObservableTree oTree)
                            oTree.MatchDescendant(action).Subscribe(observer);
                        else
                            observer.OnNext(tChild.MatchDescendant(action));
            
                            //if (c.MatchDescendant(action) is { } match)
                            //    observer.OnNext(match);
                        
                    }
                });

            });
        }



        public static IReadOnlyTree? MatchDescendant(this IReadOnlyTree tree, Predicate<IReadOnlyTree> action)
        {
            if (action(tree))
            {
                return tree;
            }
            List<IReadOnlyTree> trees = new();
            var items = tree.Items;
            //while (tree is ITree { HasMoreChildren: true })
            //{

            //}
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


        public static IEnumerable<IReadOnlyTree> MatchDescendants(this IReadOnlyTree tree, Predicate<IReadOnlyTree> action)
        {
            if (action(tree))
            {
                yield return tree;
            }
            List<IReadOnlyTree> trees = new();
            var items = tree.Items;
            //while (tree is ITree { HasMoreChildren: true })
            //{

            //}
            foreach (var child in tree.Items)
                if (child is IReadOnlyTree tChild)
                {
                    if (action(tChild))
                    {
                        yield return tChild;
                    }
                    else
                        trees.Add(tChild);
                }
                else
                    throw new Exception("c 333211");


            foreach (var c in trees)
            {
                foreach (var match in c.MatchDescendants(action))
                    yield return match;
            }
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

        public static IReadOnlyTree? Root(this IReadOnlyTree tree) => MatchAncestors(tree, t => t.IsRoot()).SingleOrDefault();
      

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