using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilityHelper
{
    public static class UriHelper
    {
        public static Uri CreateUri(string relativePath, string assemblyName)
        {
            var uri = new Uri($"pack://application:,,,{PrependForwardSlash(assemblyName)};component{PrependForwardSlash(relativePath)}");
            return uri;

            string PrependForwardSlash(string path) => path.First() == '/' ? path : "/" + path;
        }

    }
}
