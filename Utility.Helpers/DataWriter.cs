using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Utility.Helpers
{
    public static class DataWriter
    {
        public static string WriteToString(IEnumerable<KeyValuePair<string, IDictionary>> data)
        {
            using (StringWriter output = new StringWriter())
            {
                Write(data, output);
                return output.ToString();
            }
        }

        public static string WriteToString(IDictionary data)
        {
            using (StringWriter output = new StringWriter())
            {
                Write(data, output);
                output.Close();
                return output.ToString();
            }
        }

        public static void WriteToFile(IEnumerable<KeyValuePair<string, IDictionary>> data, string fileName)
        {
            using (TextWriter output = File.CreateText(fileName))
            {
                Write(data, output);
                output.Close();
            }
        }

        public static void Write(IEnumerable<KeyValuePair<string, IDictionary>> data, TextWriter output)
        {
            foreach (KeyValuePair<string, IDictionary> x in data)
            {
                output.WriteLine(x.Key);
                Write(x.Value, output);
            }
        }

        public static void Write(IDictionary data, TextWriter output)
        {
            foreach (DictionaryEntry pair in data)
            {
                output.Write((string)pair.Key);
                output.Write('\t');
                output.WriteLine((string)pair.Value);
            }
        }
    }
}