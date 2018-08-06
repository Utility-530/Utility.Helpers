

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
    using System.Reflection;

    public static class CsvHelper
    {

        public static int GetLineCount(string filename)
        {
            var text = File.OpenText(filename);
            int i = 0;
            string line = null;
            while ((line = text.ReadLine()) != null)
            {
                i++;
            }
            text.Dispose();
            return i;


        }

        public static void WriteToCsv(string csvstring, string path)
        {

            if (File.Exists(path))
                File.AppendAllText(path, new System.Text.RegularExpressions.Regex(".*\n").Replace(csvstring, "", 1));
            else
                File.WriteAllText(path, csvstring);



        }

        public static IEnumerable<string[]> GetFileLines(string filename, int skipfirst = 0)
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
                            yield return line.Split();
                        else
                            i++;
                    }
                }
            }
        }



        public static IEnumerable<T> ReadFromCsv<T>(string filename)
        {

            var x = GetFileLines(filename, 0);
            var y = x.First();

            var z = x.Skip(1).Select(_ => _.Zip(y, (a, b) => new { a, b }).ToDictionary(cc => cc.b, vv => vv.a));

            return z.ToObjects<T>();

        }



        public static string ToCSVString<T>(this IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var result = new StringBuilder();

            var props = typeof(T).GetProperties().Select(_ => _.Name);

            var cols = String.Join(",", props);

            result.AppendLine(cols);

            foreach (var row in data)
            {
                result.AppendLine(ToCommaDelimitedRow(row, properties));
            }

            return result.ToString();
        }


        public static string ToCommaDelimitedRow<T>(T obj, PropertyInfo[] properties = null)
        {
            properties = properties ?? typeof(T).GetProperties();

            var values = properties.Select(p => p.GetValue(obj, null))
                                   .Select(v => StringToCSVCell(Convert.ToString(v)));
            return string.Join(",", values);

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


        public static void CombineCsvFiles(string sourceFolder, string destinationFile, string searchPattern = "*.csv", bool isMismatched = false)
        {
            // Specify wildcard search to match CSV files that will be combined
            string[] filePaths = Directory.GetFiles(sourceFolder, searchPattern);
            if (isMismatched)
                CombineMisMatchedCsvFiles(filePaths, destinationFile);
            else
                CombineCsvFiles(filePaths, destinationFile);

        }



        public static void CombineCsvFiles(string[] filePaths, string destinationFile)
        {
            StreamWriter fileDest = new StreamWriter(destinationFile, true);

            int i;
            for (i = 0; i < filePaths.Length; i++)
            {
                string file = filePaths[i];

                string[] lines = File.ReadAllLines(file);

                if (i > 0)
                {
                    lines = lines.Skip(1).ToArray(); // Skip header row for all but first file
                }

                foreach (string line in lines)
                {
                    fileDest.WriteLine(line);
                }
            }

            fileDest.Close();
        }

        public static void CombineMisMatchedCsvFiles(string[] filePaths, string destinationFile, char splitter = ',', bool Union = true)
        {

            HashSet<string> combinedheaders = new HashSet<string>();
            int i;
            // aggregate headers
            for (i = 0; i < filePaths.Length; i++)
            {
                string file = filePaths[i];

                //if (Union)
                combinedheaders.UnionWith(File.ReadLines(file).First().Split(splitter));
                //else
                //combinedheaders.Intersect(File.ReadLines(file).First().Split(splitter));
            }

            if (combinedheaders.Contains("")) combinedheaders.Remove("");
            var hdict = combinedheaders.ToDictionary(y => y, y => new List<object>());



            string[] combinedHeadersArray = combinedheaders.ToArray();
            for (i = 0; i < filePaths.Length; i++)
            {
                var fileheaders = File.ReadLines(filePaths[i]).First().Split(splitter).Where(x => x != "").ToArray();
                var notfiledheaders = combinedheaders.Except(fileheaders);

                var fi = new FileInfo(filePaths[i]);
                if (fi.Length == 0)
                    throw new Exception($"File {fi.Name} is empty");

                File.ReadLines(filePaths[i]).Skip(1).Select(line => line.Split(splitter)).ForEach(spline =>
                {
                    for (int j = 0; j < fileheaders.Length; j++)
                    {
                        hdict[fileheaders[j]].Add(spline[j]);
                    }
                    foreach (string header in notfiledheaders)
                    {
                        hdict[header].Add(null);
                    }

                });
            }

            DataTable dt = hdict.ToDataTable();

            dt.SaveToCSV(destinationFile);
        }




        public static Dictionary<string, string> SplitCsvFilesByField(string filePath, string header, char splitter = ',')
        {

            var firstLine = File.ReadLines(filePath).First();
            int headerindex = Array.IndexOf(firstLine.Split(splitter), header);

            var fileheaders = File.ReadLines(filePath).First().Split(splitter).Where(x => x != "").ToArray();

            Dictionary<string, StringBuilder> dictbuilder = new Dictionary<string, StringBuilder>();

            StringBuilder builder = new StringBuilder();


            File.ReadLines(filePath).Skip(1).ForEach(line =>
            {
                var key = line.Split(splitter).ElementAt(headerindex);

                dictbuilder.TryGetValue(key, out StringBuilder value);
                if (value == null)
                {
                    value = new StringBuilder();
                    value.AppendLine(firstLine);
                }

                value.AppendLine(line);

                dictbuilder[key] = value;

            });



            return dictbuilder.ToDictionary(_ => _.Key, _ => _.Value.ToString());


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


