using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Utility.Interfaces.Exs;
using Utility.Interfaces.Exs.Diagrams;
using Utility.Interfaces.Methods;
using Utility.Interfaces.NonGeneric;
using Utility.Observables.Generic;
using Utility.PropertyNotifications;
using Utility.ServiceLocation;

namespace Utility.Extensions
{
    public static class ServiceHelper
    {
        public static void Observe<TParam>(this IServiceResolver serviceResolver, INodeViewModel tModel, bool includeInitial = true) where TParam : IParameter
        {
            if (tModel is not IGetName name)
            {
                throw new Exception("f 333333");
            }
            serviceResolver.Observe<TParam>(tModel.WhenReceivedFrom(a => (a as IGetValue).Value, includeInitialValue: includeInitial, includeNulls: false));
        }

        public static void ReactTo<TParam, TInput, TOutput>(this IServiceResolver serviceResolver, INodeViewModel tModel, Func<TInput, TOutput>? transformation = null) where TParam : IParameter
        {
            ReactTo<TParam, TInput>(serviceResolver, new Action<TInput>(a =>
            {
                TOutput? output = default;
                if (transformation != null)
                    output = transformation.Invoke(a);
                else if (a is TOutput x)
                    output = x;
                else if (typeof(TOutput).IsAssignableFrom(typeof(TInput)))
                {
                    output = (TOutput)(object)a;
                }
                else if (a is IConvertible && typeof(TOutput).IsPrimitive)
                {
                    output = (TOutput)Convert.ChangeType(a, typeof(TOutput));
                }
                else
                {
                    throw new InvalidCastException($"Cannot convert from {typeof(TInput)} to {typeof(TOutput)}");
                }

                (tModel as ISetValue).Value = output;
                tModel.RaisePropertyChanged(nameof(IGetValue.Value));
            }), tModel);
        }

        public static void ReactTo<TParam, TInput>(this IServiceResolver serviceResolver, Action<TInput> setAction, object? reference = null) where TParam : IParameter
        {

            var observer = new Reactives.Observer<TInput>(a =>
            {
                Globals.UI.Send((_) =>
                {
                    setAction(a);
                }, null);
            }, e => { throw e; }, () => { })
            { Reference = reference };

            serviceResolver.ReactTo<TParam, TInput>(observer);
        }

        public static void Observe<TParam>(this INodeViewModel tModel, Guid? guid = default, bool includeInitial = true) where TParam : IParameter =>
            resolve(guid).Observe<TParam>(tModel, includeInitial: includeInitial);

        public static void Observe<TParam, TObs>(this IObservable<TObs> observable, Guid? guid = default) where TParam : IParameter where TObs : class =>
            resolve(guid).Observe<TParam>(observable.Select(a => (object)a));

        public static void ReactTo<TParam>(this INodeViewModel tModel, Func<object, object>? transformation = null, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, object, object>(resolve(guid), tModel, transformation);

        public static void ReactTo<TParam, TInputOutput>(this INodeViewModel tModel, Func<TInputOutput, TInputOutput>? transformation = null, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, TInputOutput, TInputOutput>(resolve(guid), tModel, transformation);

        public static void ReactTo<TParam, TInput, TOutput>(this INodeViewModel tModel, Func<TInput, TOutput>? transformation = null, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, TInput, TOutput>(resolve(guid), tModel, transformation);

        public static void ReactTo<TParam>(this INodeViewModel tModel, Action<object> setAction, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, object>(resolve(guid), setAction, tModel);

        public static void ReactTo<TParam, TInputOutput>(this INodeViewModel tModel, Action<TInputOutput> setAction, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, TInputOutput>(resolve(guid), setAction, tModel);

        public static void ReactTo<TParam, TInput, TOutput>(this INodeViewModel tModel, Action<TInput> setAction, Guid? guid = default) where TParam : IParameter =>
            ReactTo<TParam, TInput>(resolve(guid), setAction, tModel);

        private static IServiceResolver resolve(Guid? guid = default) => Globals.Resolver.Resolve<IServiceResolver>(guid?.ToString()) ?? throw new Exception("ServiceResolver not found");

        private class EqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y) => x == y;

            public int GetHashCode([DisallowNull] object obj) => obj.GetHashCode();
        }
    }
}