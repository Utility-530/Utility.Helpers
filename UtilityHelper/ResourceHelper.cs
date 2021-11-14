using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utility
{
    public class ResourceHelper
    {
        public static Stream? GetEmbeddedResource(string endsWith, Assembly? assembly = default)
        {
            assembly ??= Assembly.GetEntryAssembly();

            if (assembly == null)
            {
                throw new("Unable to retrieve Embedded Resource: no entry assembly");
            }

            string resourceName = assembly.GetManifestResourceNames().First(str => str.EndsWith(endsWith));
            Stream? stream = assembly.GetManifestResourceStream(resourceName);

            return stream;
        }

        public static Bitmap GetEmbeddedResourceAsBitmap(string endsWith, Assembly? assembly = default)
        {
            var resource = GetEmbeddedResource(endsWith, assembly);

            if (resource == null)
            {
                throw new("Unable to retrieve Embedded Resource: resource is null");
            }
            return new (resource);
        }
    }
}