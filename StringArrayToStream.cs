using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class StringArrayToStream
    {

        public static Stream ToStream(this string[] str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            foreach (var s in str) writer.WriteLine(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
