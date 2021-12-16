using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public class EqualityComparerFactory
    {
        private sealed class Impl<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> Eq;
            private readonly Func<T, int> HashFunc;

            public Impl(Func<T, T, bool> eq, Func<T, int> hashFunc)
            {
                Eq = eq;
                HashFunc = hashFunc;
            }

            public bool Equals(T left, T right)
            {
                return Eq(left, right);
            }

            public int GetHashCode(T obj)
            {
                return HashFunc(obj);
            }
        }

        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> eq, Func<T, int> hashFunc)
        {
            return new Impl<T>(eq, hashFunc);
        }

        public static IEqualityComparer<T> Create<T, R>(Func<T, R> eq, Func<T, int>? hashFunc = null) where R : notnull
        {
            return new Impl<T>((a, b) => eq(a).Equals(eq(b)), hashFunc ?? (a => 0));
        }

        public static Func<T, int> GuidHasher<T>(Func<T, Guid> guidPredicate)
        {
            return t => guidPredicate(t).ToByteArray().Take(3).Aggregate(1, (i, b) => i * b);
        }
    }

    //var comparer = EqualityComparerFactory.Create<TileTagModel>((a, b) => a.Name == b.Name && a.IsDefault == b.IsDefault, model => model.Name.GetHashCode() + model.IsDefault.GetHashCode());
    //Assert.That(tags, Is.EquivalentTo(expected).Using(comparer));
}