using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Utility.Abstract;

namespace Utility
{
    public static class CsvHelper
    {
        //public static MemoryStream ToMemoryStream(this IEnumerable records, ClassMap classMap = null, bool useHeaderAttribute = false)
        //{
        //    var memoryStream = new MemoryStream();
        //    var writer = new StreamWriter(memoryStream);
        //    var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        //    if (classMap != null)
        //        csv.Configuration.RegisterClassMap(classMap);
        //    WriteRecordsCustom(records, csv, useHeaderAttribute);

        //    writer.Flush(); // flush the buffered text to stream
        //    memoryStream.Seek(0, SeekOrigin.Begin); // reset stream position
        //    return memoryStream;
        //}

        //public static T[] FormArrayFromPath<T>(string path, ClassMap classMap = null)
        //{
        //    using (var reader = new StreamReader(path))
        //    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        //    {
        //        if (classMap != null)
        //            csv.Configuration.RegisterClassMap(classMap);
        //        var records = csv.GetRecords<T>();
        //        return records.ToArray();
        //    }
        //}

        public static MemoryStream ToMemoryStream<T>(this IEnumerable<T> records, bool useHeaderAttribute = false)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            WriteRecordsCustom(records, csv, useHeaderAttribute);
            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin); // reset stream position
            return memoryStream;
        }


        public static string ToString<T>(this IEnumerable<T> records, bool useHeaderAttribute = false)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    WriteRecordsCustom(records, csv, useHeaderAttribute);

                    writer.Flush();
                    return StreamHelper.ToString(memoryStream);
                }
            }
        }

        static void WriteRecordsCustom<T>(IEnumerable<T> records, CsvWriter csv, bool useHeaderAttribute)
        {
            if (useHeaderAttribute)
            {
                foreach (var property in typeof(T).GetProperties())
                {
                    csv.WriteField(property.GetAttributeProperty<CsvHeaderAttribute>(a => a.Header));
                }

                csv.NextRecord();

                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }
            else
            {
                csv.WriteRecords(records);
            }
        }

        static void WriteRecordsCustom(IEnumerable records, CsvWriter csv, bool useHeaderAttribute)
        {
            if (useHeaderAttribute)
            {
                var enumerator = records.GetEnumerator();
                enumerator.MoveNext();
                var type = enumerator.Current.GetType();
                foreach (var property in type.GetProperties())
                {
                    csv.WriteField(property.GetAttributeProperty<CsvHeaderAttribute>(a => a.Header));
                }


                csv.NextRecord();

                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }
            else
            {
                csv.WriteRecords(records);
            }
        }
    }
}