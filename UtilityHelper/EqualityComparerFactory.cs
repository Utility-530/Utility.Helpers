using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public class EqualityComparerFactory
    {
        private sealed class Impl<T> : IEqualityComparer<T>
        {
            private readonly Func<T?, T?, bool> Eq;
            private readonly Func<T, int> HashFunc;

            public Impl(Func<T?, T?, bool> eq, Func<T, int> hashFunc)
            {
                Eq = eq;
                HashFunc = hashFunc;
            }

            public bool Equals(T? left, T? right)
            {
                return Eq(left, right);
            }

            public int GetHashCode(T obj)
            {
                return HashFunc(obj);
            }
        }


        public static IEqualityComparer<T> Create<T>(Func<T?, T?, bool> eq, Func<T, int>? hashFunc = default)
        {
            return new Impl<T>(eq, hashFunc ?? (a => a?.GetHashCode() ?? 0));
        }

        public static IEqualityComparer<T> Create<T, R>(Func<T, R> eq, Func<T, int>? hashFunc = default)
        {
            return new Impl<T>((a, b) => a != null && b != null && (eq(a)?.Equals(eq(b)) ?? false), hashFunc ?? (a => a?.GetHashCode() ?? 0));
        }

        public static IEqualityComparer<T> Create<T>(Func<T, int>? hashFunc = default) where T : IEquatable<T>
        {
            return new Impl<T>((a, b) => a != null && b != null && a.Equals(b), hashFunc ?? (a => a?.GetHashCode() ?? 0));
        }
    }

    //var comparer = EqualityComparerFactory.Create<TileTagModel>((a, b) => a.Name == b.Name && a.IsDefault == b.IsDefault, model => model.Name.GetHashCode() + model.IsDefault.GetHashCode());
    //Assert.That(tags, Is.EquivalentTo(expected).Using(comparer));
}