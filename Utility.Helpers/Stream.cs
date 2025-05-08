﻿using System;
using System.IO;

namespace Utility.Helpers
{
    public static class StreamHelper
    {
        public static MemoryStream ToMemoryStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static void OverWriteFile(this Stream stream, string path)
        {
            Directory.GetParent(path).Create();
            using FileStream fileStream = new (path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fileStream.SetLength(0);
            stream.CopyTo(fileStream);
        }


        public static FileStream ToFileStream(this string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return stream;
        }

        [Obsolete("too easily confused with other methods. Use AsString")]
        public static string ToString(this Stream stream)
        {
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }     
        
        public static string AsString(this Stream stream)
        {
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }
    }
}