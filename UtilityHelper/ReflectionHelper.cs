using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
   public class ReflectionHelper
   {
      public static IEnumerable<string> SelectPropertyNamesOfDeclaringType<T>()
      
         => typeof(T).GetProperties()
            .Where(x => x.DeclaringType == typeof(T))
            .Select(info => info.Name);


        public static IEnumerable<Type> SelectAssignableTypes<T, TBase>()

         => typeof(T).Assembly.GetTypes()
                .Where(x => typeof(TBase).IsAssignableFrom(x));

    }
}
