using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace UtilityHelper
{
    /// <summary>
    /// Class that contains logic to deserialize an XML or JSON file to an Update object
    /// </summary>
    public static class XmlSerialisation
    {
        public static void WriteFormattedXml(string filePath, string rawStringXML)
        {
            using (MemoryStream ms = PrettifyXml(rawStringXML))
            {
                ms.Seek(0, SeekOrigin.Begin);

                using (FileStream fs = new FileStream(filePath, File.Exists(filePath) ? FileMode.Truncate : FileMode.CreateNew, FileAccess.Write))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }
            }

            MemoryStream PrettifyXml(string xml)
            {
                var element = XElement.Parse(xml);

                var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, NewLineOnAttributes = true };

                var stream = new MemoryStream();
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    element.Save(xmlWriter);
                }

                return stream;
            }
        }

        /// <summary>
        /// Deserialize XML data into an T object
        /// </summary>
        /// <param name="data">The XML data that should be deserialized</param>
        /// <returns>The T object that was deserialized</returns>
        public static T DeserializeXml<T>(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException(nameof(data));

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(data);
                    writer.Flush();
                    stream.Position = 0;
                    return (T)serializer.Deserialize(stream);
                }
            }
        }

        public static string Serialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        /// <summary>
        /// Deserialize XML data into an T object asynchronously
        /// </summary>
        /// <param name="data">The XML data that should be deserialized</param>
        /// <returns>The T object that was deserialized or null if an error occurred</returns>
        public static async Task<T> DeserializeXmlAsync<T>(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException(nameof(data));

            return await Task.Run(() =>
            {
                var serializer = new XmlSerializer(typeof(T));
                T deserialize;
                // Declare an object variable of the type to be deserialized.
                using (Stream reader = GenerateStreamFromString(data))
                {
                    // Call the Deserialize method to restore the object's state.
                    deserialize = (T)serializer.Deserialize(reader);
                }
                return deserialize;
            });

            Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }
    }
}