

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{

    //using Microsoft.VisualBasic.FileIO;
    using System.Data;
    using System.IO;

    public static class CsvHelper
    {



        public static void WriteToCsv(string csvstring, string path )
        {
            
            if (File.Exists(path))
                File.AppendAllText(path, new System.Text.RegularExpressions.Regex(".*\n").Replace(csvstring,"",1));
            else
                File.WriteAllText(path, csvstring);



        }

        public static IEnumerable<string> GetFileLines(string filename, int skipfirst=0)
        {
            using (var stream = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    //reader.ReadLine(); // skip one line
                    string line; int i = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (i > skipfirst - 1)
                            yield return line;
                        else
                            i++;
                    }
                }
            }
        }





        public static string CreateCSVTextFile<T>(this IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var result = new StringBuilder();

            var props = typeof(T).GetProperties().Select(_ => _.Name);

            var cols = String.Join(",", props);

            result.AppendLine(cols);

            foreach (var row in data)
            {
                var values = properties.Select(p => p.GetValue(row, null))
                                       .Select(v => StringToCSVCell(Convert.ToString(v)));
                var line = string.Join(",", values);

                result.AppendLine(line);
            }

            return result.ToString();
        }




        public static string CreateCSVTextFile<T>(T obj)
        {
            var properties = typeof(T).GetProperties();
            var result = new StringBuilder();


            var values = properties.Select(p => p.GetValue(obj, null))
                                   .Select(v => StringToCSVCell(Convert.ToString(v)));
            var line = string.Join(",", values);
            result.AppendLine(line);


            return result.ToString();
        }
        private static string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }




        //https://github.com/22222/CsvTextFieldParser
        //public static IEnumerable<string[]> Parse(string path)
        //{


        //    using (TextFieldParser csvParser = new TextFieldParser(path))
        //    {
        //        //csvParser.CommentTokens = new string[] { "#" };
        //        csvParser.SetDelimiters(new string[] { "," });
        //        csvParser.HasFieldsEnclosedInQuotes = false;

        //        // Skip the row with the column names
        //        csvParser.ReadLine();

        //        while (!csvParser.EndOfData)
        //        {
        //            // Read current line fields, pointer moves to the next line.
        //            yield return csvParser.ReadFields();

        //        }
        //    }



        //}






    }
}


