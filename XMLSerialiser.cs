
using System.Runtime.Serialization;
using System.IO;
using System.Windows;




namespace System.Xml.Serialization
{


    public static class ReadWrite
    {
        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static bool WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {

            try
            {
                DataContractSerializerSettings contractsettings = new DataContractSerializerSettings();
                // this ensures nodes and edges use same reference for object  i.e one object =one node
                contractsettings.PreserveObjectReferences = true;

                DataContractSerializer serializer = new DataContractSerializer(typeof(T), contractsettings);



                var settings = new XmlWriterSettings { Indent = true };

                // (objectToWrite as Models.DiagramModel).Transfer();
                using (var w = XmlWriter.Create(filePath, settings))
                {
                    serializer.WriteObject(w, objectToWrite);

                }


            }
            catch
            {
                //MessageBox.Show("Unable to write to file");
                return false;
            }
            finally
            {

                //if (writer != null)
                //    writer.Close();
            }
            return true;

        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {


            // Type[] knownTypes = new Type[4];


            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            try
            {
                if (!File.Exists(filePath))
                    return new T();
                using (FileStream fs = File.Open(filePath, FileMode.Open))
                {
                    T model = (T)serializer.ReadObject(fs);
                    // model.SettingsFilename = filename;
                    return model;
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Failed to load settings from " + filePath + " !: " + ex.ToString());
                //MessageBox.Show("Failed to load settings from " + filename + " !: " + ex.InnerException.Message);
                return new T();
            }

        }
    }



}
