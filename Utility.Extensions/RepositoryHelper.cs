using System.Collections;
using Utility.Interfaces.Generic.Data;
using Utility.Interfaces.NonGeneric.Data;
using Utility.Persists;

namespace Utility.Extensions
{
    namespace Generic
    {
        public static class RepositoryHelper
        {
            public static IEnumerable<T> FindAll<T>(this IRepository<T, IQuery, IEnumerable<T>> repository)
            {
                return repository.FindManyBy(new AllQuery());
            }
        }
    }
    public static class RepositoryHelper
    {
        public static IEnumerable All(this IRepository repository)
        {
            return repository.FindManyBy(new AllQuery());
        }
        public static IEnumerable Clear(this IRepository repository)
        {
            return repository.RemoveManyBy(new AllQuery());
        }

        public static IEnumerable<T> All<T>(this IRepository repository)
        {
            return repository.FindManyBy(new AllQuery()).Cast<T>();
        }

        public static int Count(this IRepository repository)
        {
            return (int)repository.FindBy(new CountQuery());
        }

        public static T First<T>(this IRepository repository)
        {
            return (T)repository.FindBy(new FirstQuery());
        }

        public static void Upsert<T>(this IRepository repository)
        {
            throw new NotImplementedException();
        }
    }
}