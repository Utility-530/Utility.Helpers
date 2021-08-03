using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UtilityHelper
{
    public class ResourceHelper
    {
        public static Stream GetEmbeddedResource(string endsWith, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetEntryAssembly();

            string resourceName = assembly.GetManifestResourceNames()
                .First(str => str.EndsWith(endsWith));
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            return stream;
        }
    }
}