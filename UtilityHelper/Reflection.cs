using System.Collections.Generic;
using System.Linq;

namespace UtilityHelper
{
    public class ReflectionHelper
    {
        public static IEnumerable<string> SelectPropertyNamesOfDeclaringType<T>()

           => typeof(T).GetProperties()
              .Where(x => x.DeclaringType == typeof(T))
              .Select(info => info.Name);
    }
}
