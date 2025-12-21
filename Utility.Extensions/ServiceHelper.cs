using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using LanguageExt.ClassInstances;
using Utility.Interfaces.Exs;
using Utility.Interfaces.Exs.Diagrams;
using Utility.Interfaces.Methods;
using Utility.Interfaces.NonGeneric;
using Utility.Models.Trees;
using Utility.Nodes;
using Utility.Observables.Generic;
using Utility.PropertyNotifications;
using Utility.ServiceLocation;
using Utility.Interfaces;

namespace Utility.Extensions
{
    public class PropertyNode : IPropertyNode
    {
        public PropertyNode()
        {
        }

        public ICollection<object> Outputs { get; } = new List<object>();
        public ICollection<object> Inputs { get; } = new List<object>();
    }

    public interface IPropertyNode
    {
        ICollection<object> Outputs { get; }
        ICollection<object> Inputs { get; }
    }

    public class PropertyReceiver<TInput> : IObserver<object>
    {
        private readonly INodeViewModel nodeViewModel;

        public PropertyReceiver(INodeViewModel nodeViewModel)
        {
            this.nodeViewModel = nodeViewModel;
        }

        public required string Name { get; set; }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(object value)
        {
            (nodeViewModel as ISetValue).Value = value;
            nodeViewModel.RaisePropertyChanged(nameof(IGetValue.Value));
        }
    }

    public static class ServiceHelper
    {
        public static void Observe<TParam>(this IServiceResolver serviceResolver, INodeViewModel tModel, bool includeInitial = true) where TParam : IParameter
        {
            if (tModel is not IGetName name)
            {
                throw new Exception("f 333333");
            }
            var propertyObservable = tModel.WhenReceivedFrom(a => (a as IGetValue).Value, includeInitialValue: includeInitial, includeNulls: false);
            IPropertyNode propertyNode;
            if (tModel.Data() is not IPropertyNode node)
            {
                propertyNode = new PropertyNode() { };
                tModel.SetData(propertyNode);
            }
            else
            {
                propertyNode = node;
            }
            if(propertyNode.Inputs.Contains(propertyObservable, new ObserveEqualityComparer()))
            {
                return;
            }
            propertyNode.Inputs.Add(propertyObservable);
            serviceResolver.Observe<TParam>(propertyObservable);
        }

        public static void ReactTo<TParam, TInput, TOutput>(this IServiceResolver serviceResolver, INodeViewModel tModel, Func<TInput, TOutput>? transformation = null) where TParam : IParameter
        {
       
            IPropertyNode propertyNode;
            if (tModel.Data() is not IPropertyNode node)
            {
                propertyNode = new PropertyNode();
                tModel.SetData(propertyNode);
            }
            else
            {
                propertyNode = node;
            }

            var propertyReceiver = new PropertyReceiver<TOutput>(tModel) { Name = nameof(ISetValue.Value) };
            if (propertyNode.Inputs.Contains(propertyReceiver, new ObservableEqualityComparer()))
            {
                return;
            }
            propertyNode.Outputs.Add(propertyReceiver);
            serviceResolver.ReactTo<TParam, TInput>(propertyReceiver);
        }

        public static void ReactTo<TParam, TInput>(this IServiceResolver serviceResolver, Action<TInput> setAction, object? reference = null) where TParam : IParameter
        {
            var observer = new Reactives.Observer<object>(a =>
            {
                Globals.UI.Send((_) =>
                {
                    setAction((TInput)a);
                }, null);
            }, e => { throw e; }, () => { })
            { Reference = reference };

            serviceResolver.ReactTo<TParam, TInput>(observer);
        }

        public static void Observe<TParam>(this INodeViewModel tModel, Guid? guid = default, bool includeInitial = true) where TParam : IParameter =>
            resolve(guid).Observe<TParam>(tModel, includeInitial: includeInitial);

        public static void Observe<TParam, TObs>(this IObservable<TObs> observable, Guid? guid = default) where TParam : IParameter /*where TObs : class*/ =>
            resolve(guid).Observe<TParam>(observable.Select(a => (object)a));

        // place the transformation after guid parameter to avoid ambiguity between 
        // transformation Func and setAction Action
        public static void ReactTo<TParam>(this INodeViewModel tModel, Guid? guid = default, Func<object, object>? transformation = null) where TParam : IParameter =>
            ReactTo<TParam, object, object>(resolve(guid), tModel, transformation);

        public static void ReactTo<TParam, TInputOutput>(this INodeViewModel tModel, Guid? guid = default, Func<TInputOutput, TInputOutput>? transformation = null) where TParam : IParameter =>
            ReactTo<TParam, TInputOutput, TInputOutput>(resolve(guid), tModel, transformation);

        public static void ReactTo<TParam, TInput, TOutput>(this INodeViewModel tModel, Guid? guid = default, Func<TInput, TOutput>? transformation = null) where TParam : IParameter =>
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

    internal class ObservableEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode([DisallowNull] object obj)
        {
            throw new NotImplementedException();
        }
    }

    internal class ObserveEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode([DisallowNull] object obj)
        {
            throw new NotImplementedException();
        }
    }
}