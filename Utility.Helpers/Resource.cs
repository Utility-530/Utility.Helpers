﻿using System.IO;
using System.Linq;
using System.Reflection;

namespace Utility.Helpers
{
    public class ResourceHelper
    {
        public static Stream GetEmbeddedResource(string endsWith, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetEntryAssembly();

            var names = assembly.GetManifestResourceNames();
            string resourceName = names.First(str => str.EndsWith(endsWith));
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            return stream;
        }
    }
}