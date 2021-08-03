using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityHelper;

namespace UtilityHelper
{
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using UtilityHelper.Generic;

    public static class CsvHelper
    {
        public static string[] GetColumn(string path, string field, char delimiter = ',')
        {
            var temp = File.ReadAllLines(path);
            List<string> myExtraction = new List<string>();

            int headerline = temp.First().Split(delimiter).ToList().IndexOf(field);

            foreach (string line in temp)
            {
                var delimitedLine = line.Split(delimiter); //set ur separator, in this case tab

                myExtraction.Add(delimitedLine[headerline]);
            }
            return myExtraction.ToArray();
        }

        public static int GetLineCount(string filename)
        {
            var text = File.OpenText(filename);
            int i = 0;           
            while (text.ReadLine() is string)
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

        public static IEnumerable<string[]> GetFileLines(string filename, int skipfirst = 0, char splitchar = ',')
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
                            yield return line.Split(splitchar);
                        else
                            i++;
                    }
                }
            }
        }

        public static IEnumerable<T> ReadFromCsv<T>(string filename, char splitchar = ',')
        {
            var x = GetFileLines(filename, 0, splitchar);
            var y = x.First();

            var z = x.Skip(1).Select(_ => _.Zip(y, (a, b) => new { a, b }).ToDictionary(cc => cc.b, vv => vv.a));

            return z.MapToMany<T>();
        }

        public static string ToCSVString(this IEnumerable data)
        {
            var first = UtilityHelper.NonGeneric.Linq.First(data);
            var type = first.GetType();
            var properties = type.GetProperties();
            var result = new StringBuilder();

            var props = properties.Select(_ => _.Name);
            List<string> rows = new List<string>();
            foreach (var row in data)
            {
                rows.Add(ToCommaDelimitedRow(row, properties));
            }
            return ToCSVString(props.ToArray(), rows);
        }

        public static string ToCSVString<T>(this IEnumerable<T> data)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var result = new StringBuilder();
            var props = properties.Select(_ => _.Name);

            return ToCSVString(props.ToArray(), data.Select(_ =>            ToCommaDelimitedRow(_, properties)));
        }

        public static string ToCSVString(string[] fields, IEnumerable<string> rows)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(string.Join(",", fields));

            foreach (var row in rows)
            {
                result.AppendLine(row);
            }

            return result.ToString();
        }

        public static string ToCommaDelimitedRow<T>(T obj, PropertyInfo[]? properties = null)
        {
            return (properties ??= typeof(T).GetProperties())
                                   .Select(p => p.GetValue(obj, null))
                                   .Select(v => StringToCSVCell(Convert.ToString(v)))
                                   .Join(",");
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
            var hdict = combinedheaders.ToDictionary(y => y, y => new List<object?>());

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

            System.Data.DataTable dt = hdict.ToDataTable();

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

        public static void WriteToFile<T>(this T[][] data, string file)
        {
            using (StreamWriter outfile = new StreamWriter(file))
            {
                foreach (var line in data)
                {
                    outfile.WriteLine(String.Join(",", line));
                }
            }
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