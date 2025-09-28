using Splat;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Utility.Interfaces.Exs;
using Utility.Interfaces.Exs.Diagrams;
using Utility.Interfaces.NonGeneric;
using Utility.Models;
using Utility.Observables.Generic;
using Utility.PropertyNotifications;
using Utility.ServiceLocation;

namespace Utility.Extensions
{
    public static class ServiceHelper
    {
        public static void Observe<TParam>(this IServiceResolver serviceResolver, INodeViewModel tModel) where TParam : IMethodParameter
        {
            //var observable = new Reactives.Observable<object>(
            //    [tModel.WhenReceivedFrom(a => (a as IGetValue).Value, includeNulls: false),
            //tModel.WithChangesTo(a => (a as IGetValue).Value, includeNulls: false, includeInitialValue: false)]);


            if (tModel is not IGetName name)
            {
                throw new Exception("f 333333");
            }
            serviceResolver.Observe<TParam>(tModel.WhenReceivedFrom(a => (a as IGetValue).Value, includeNulls: false));
        }

        public static void ReactTo<TParam>(this IServiceResolver serviceResolver, INodeViewModel tModel, Func<object, object>? transformation = null, Action<object>? setAction = null) where TParam : IMethodParameter
        {
            setAction ??= (a) => (tModel as ISetValue).Value = a;

            var observer = new Reactives.Observer<object>(a => setAction(transformation != null ? transformation.Invoke(a) : a), e => { }, () => { }) { Reference = tModel };
            if (tModel is not IGetName name)
            {
                throw new Exception("f 333333");
            }

            serviceResolver.ReactTo<TParam>(observer);
        }

        public static INodeViewModel ToValueModel<T>(this IObservable<T> observable)
        {
            var valueModel = new Model<T>() { Name = typeof(T).Name };
            observable.Subscribe(a => valueModel.Value = a);
            return valueModel;
        }

        public static IObservable<T> Observe<TParam, T>(this IObservable<T> observable) where TParam : IMethodParameter
        {
            observable.ToValueModel().Observe<TParam>();
            return observable;
        }

        public static void Observe<TParam>(this INodeViewModel tModel) where TParam : IMethodParameter =>
            Utility.Globals.Resolver.Resolve<IServiceResolver>().Observe<TParam>(tModel);

        public static void ReactTo<TParam>(this INodeViewModel tModel, Func<object, object>? transformation = null, Action<object>? setAction = null) where TParam : IMethodParameter =>
            Globals.Resolver.Resolve<IServiceResolver>().ReactTo<TParam>(tModel, transformation, setAction);

        private class EqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y) => x == y;
            public int GetHashCode([DisallowNull] object obj) => obj.GetHashCode();
        }
    }
}
