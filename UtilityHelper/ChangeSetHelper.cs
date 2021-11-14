using System;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Utility.Abstract;
using System.Collections.ObjectModel;

namespace Utility
{
    public record KeyObservableList<T, TGroupKey>(TGroupKey Key, IObservableList<T> ObservableList);

    public record KeyObservableCache<T, TKey>(TKey Key, IObservableCache<T, TKey> Cache) where TKey : notnull;

    public record KeyObservableCache<T, TGroupKey, TKey>(TGroupKey Key, IObservableCache<T, TKey> Cache) where TKey : notnull;

    public record KeyCollection< TOut>(string Key, ReadOnlyObservableCollection<TOut> Collection);

    public static class ChangeSetHelper
    {
        public static IObservable<IChangeSet<TOut, TValue>>
           FilterAndSelect<T, TKey, TValue, TOut>(
           this IObservable<IChangeSet<T, TKey>> observable,
           IObservable<Func<T, bool>> predicate,
           Func<TOut, TValue> keySelector,
           Func<T?, IObservableCache<TOut, TValue>> selector)
             where TKey : notnull
             where TValue : notnull
        {
            var collection = observable
            .Filter(predicate)
            .ToCollection()
            .Where(a => a != null)
            .Select(a => selector(a.FirstOrDefault()) ?? new SourceCache<TOut, TValue>(keySelector))
            .SelectMany(a => a.Connect());

            return collection;
        }

        public static IObservable<IChangeSet<TOut, TValue>>
       FilterAndSelect<T, TKey, TValue, TOut>(
       this IObservable<IChangeSet<T, TKey>> observable,
       Func<T, bool> predicate,
       Func<TOut, TValue> keySelector,
       Func<T?, IObservableCache<TOut, TValue>> selector)
         where TKey : notnull
         where TValue : notnull
        {
            var collection = observable
            .Filter(a => predicate(a))
            .ToCollection()
            .Where(a => a != null)
            .Select(a => selector(a.FirstOrDefault()) ?? new SourceCache<TOut, TValue>(keySelector))
            .SelectMany(a => a.Connect());

            return collection;
        }

        public static IObservable<IChangeSet<KeyObservableCache<T, TGroupKey, TKey>, TGroupKey>>
            SelectGroups<T, TGroupKey, TKey>(IObservable<T> observable, IScheduler scheduler)
            where T : UtilityInterface.Generic.IKey<TKey>, IGroupKey<TGroupKey>
             where TGroupKey : notnull
             where TKey : notnull
        {
            var transforms = observable
                          .ObserveOn(scheduler)
                          .SubscribeOn(scheduler)
                        .ToObservableChangeSet(a => a.Key)
                        .Group(a => a.GroupKey)
                        .Transform(a => new KeyObservableCache<T, TGroupKey, TKey>(a.Key, a.Cache));

            return transforms;
        }

        public static IObservable<IChangeSet<KeyObservableCache<T, TGroupKey, TKey>, TGroupKey>>
    SelectGroups<T, TGroupKey, TKey>(IObservable<T> observable, Func<T, TKey> funcKey, Func<T, TGroupKey> funcGroupKey, IScheduler scheduler)  
     where TGroupKey : notnull
     where TKey : notnull
        {
            var transforms = observable
                          .ObserveOn(scheduler)
                          .SubscribeOn(scheduler)
                        .ToObservableChangeSet(funcKey)
                        .Group(funcGroupKey)
                        .Transform(a => new KeyObservableCache<T, TGroupKey, TKey>(a.Key, a.Cache));

            return transforms;
        }

        public static IObservable<IChangeSet<KeyObservableList<T, TGroupKey>>>
            SelectGroupKeyGroups<T, TGroupKey, TKey>(IObservable<T> observable, IScheduler scheduler)
            where T : UtilityInterface.Generic.IKey<TKey>, IGroupKey<TGroupKey>
             where TGroupKey : notnull
        {
            var transforms = observable
                          .ObserveOn(scheduler)
                          .SubscribeOn(scheduler)
                          .ToObservableChangeSet()
                          .GroupOn(a => a.GroupKey)
                          .Transform(a => new KeyObservableList<T, TGroupKey>(a.GroupKey, a.List));

            return transforms;
        }

        public static IObservable<IChangeSet<KeyCollection<TOut>>> SelectKeyGroups<TKey, TOut>(IObservable<TKey> states, IScheduler scheduler, Func<TKey, TOut> transform)
              where TKey : UtilityInterface.Generic.IKey<string>
        {
            var keyGroups = states
                .ObserveOn(scheduler)
                .SubscribeOn(scheduler)
                .ToObservableChangeSet()
                .GroupOn(a => a.Key)
                .Transform(a =>
                {
                    a.List.Connect().Transform(transform).Bind(out var gitems).Subscribe();
                    return new KeyCollection<TOut>(a.GroupKey, gitems);
                });

            return keyGroups;
        }

        public static IObservable<IChangeSet<KeyObservableList<T, TKey>>> SelectGroups<T, TKey>(IObservable<T> observable, IScheduler scheduler) 
            where T : UtilityInterface.Generic.IKey<TKey>
             where TKey : notnull
        {
            var transforms = observable
                .ObserveOn(scheduler)
                .SubscribeOn(scheduler)
                .ToObservableChangeSet()
                .GroupOn(a => a.Key)
                .Transform(a => new KeyObservableList<T, TKey>(a.GroupKey, a.List));

            return transforms;
        }
        public static IObservable<IChangeSet<KeyCollection<TOut>, TGroupKey>> SelectGroupKeyGroups<TKey, TGroupKey, TOut>(
            IObservable<TKey> states, IScheduler scheduler, Func<TKey, TOut> func)
            where TKey : notnull, UtilityInterface.Generic.IKey<string>, IGroupKey<TGroupKey>
            where TGroupKey : notnull
        {
            var keyGroups = states
                .ObserveOn(scheduler)
                .SubscribeOn(scheduler)
                .ToObservableChangeSet(a => a.Key)
                .Group(a => a.GroupKey)
                .Transform(a =>
                {
                    a.Cache
                    .Connect()
                    .Transform(func)
                    .Bind(out var gitems).Subscribe();

                    return new KeyCollection<TOut>(a.Key.ToString()?? throw new Exception("Can't convert to string"), gitems);
                });

            return keyGroups;
        }



        //public static IObservable<IChangeSet<TOut,TKey>> FilterAndSelect<T,TKey, TOut>(
        //    this IObservable<IChangeSet<T>> observable,
        //    Func<T, bool> predicate,
        //    Func<TOut, TKey> keySelector,
        //    Func<T?, IEnumerable<TOut>?> selector)
        //{
        //    var collection = observable
        //                           .Filter(a => predicate(a))
        //                           .ToCollection()
        //                           .WhereNotNull()
        //                           .SelectMany(a => selector(a.FirstOrDefault()) ?? Array.Empty<TOut>())
        //                           .ToObservableChangeSet(keySelector);

        //    return collection;
        //}

        //public static IObservable<IChangeSet<KeyCache<T, TGroupKey, TKey>, TGroupKey>>
        //SelectGroups<T, TGroupKey, TKey>(IObservable<T> observable, IScheduler scheduler)
        //where T : UtilityInterface.Generic.IKey<TKey>, IGroupKey<TGroupKey>
        //{
        //    var transforms = observable
        //                  .ObserveOn(scheduler)
        //                  .SubscribeOn(scheduler)
        //                  .ToObservableChangeSet()
        //                  .GroupOn(a => a.GroupKey)
        //                  .Transform(a => new KeyCache<T,TGroupKey,TKey>(a.GroupKey, a.List));

        //    return transforms;
        //}

    }
}
