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

        static void reactTo<TParam, TInput, TOutput>(this IServiceResolver serviceResolver, INodeViewModel tModel, Func<TInput, TOutput>? transformation = null, Action<TInput>? setAction = null) where TParam : IParameter
        {
            setAction ??= new Action<TInput>(a =>
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
            });

            var observer = new Reactives.Observer<TInput>(a =>
            {
                Globals.UI.Send((_) =>
                {
                    setAction(a);
                }, null);
            }, e => { throw e; }, () => { })
            { Reference = tModel };
            if (tModel is not IGetName name)
            {
                throw new Exception("f 333333");
            }

            serviceResolver.ReactTo<TParam, TInput>(observer);
        }

        public static void Observe<TParam>(this INodeViewModel tModel, Guid? guid = default, bool includeInitial = true) where TParam : IParameter =>
            resolve(guid).Observe<TParam>(tModel, includeInitial: includeInitial);
        public static void Observe<TParam, TObs>(this IObservable<TObs> observable, Guid? guid = default) where TParam : IParameter =>
            resolve(guid).Observe<TParam>(observable.Select(a => (object)a));
        public static void ReactTo<TParam>(this INodeViewModel tModel, Action<object>? setAction = null, Guid? guid = default) where TParam : IParameter =>
            reactTo<TParam, object, object>(resolve(guid), tModel, a => a, setAction);
        public static void ReactTo<TParam, TInputOutput>(this INodeViewModel tModel, Action<TInputOutput>? setAction = null, Guid? guid = default) where TParam : IParameter =>
            reactTo<TParam, TInputOutput, TInputOutput>(resolve(guid), tModel, a => a, setAction);
        public static void ReactTo<TParam, TInput, TOutput>(this INodeViewModel tModel, Func<TInput, TOutput>? transformation = null, Action<TInput>? setAction = null, Guid? guid = default) where TParam : IParameter =>
            reactTo<TParam, TInput, TOutput>(resolve(guid), tModel, transformation, setAction);

        private static IServiceResolver resolve(Guid? guid = default) => Globals.Resolver.Resolve<IServiceResolver>(guid?.ToString()) ?? throw new Exception("ServiceResolver not found");

        private class EqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y) => x == y;
            public int GetHashCode([DisallowNull] object obj) => obj.GetHashCode();
        }
    }
}
