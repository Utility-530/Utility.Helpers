using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

//using LumenWorks.Framework.IO.Csv;

namespace UtilityHelper
{
    public static class DataTableHelper
    {
        //public static DataTable ReadCsvToDataTable(string path)
        //{
        //    DataTable csvTable = new DataTable();
        //    // open the file "data.csv" which is a CSV file with headers
        //    using (CsvReader csvReader = new CsvReader(
        //                           new StreamReader(path), true))
        //    {
        //        csvTable.Load(csvReader);
        //    }
        //    return csvTable;

        //}

        //https://stackoverflow.com/questions/15293653/coverting-list-of-dictionary-to-datatable
        // answered Mar 8 '13 at 14:57 John Kraft
        public static System.Data.DataTable ToDataTableAsDictionary<T>(this IList<Dictionary<string, T>> list)
        {
            System.Data.DataTable result = new System.Data.DataTable();
            if (list == null || list.Count == 0)
                return result;

            var columnNames = list.SelectMany(dict => dict.Keys).Distinct();
            result.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
            foreach (Dictionary<string, T> item in list)
            {
                var row = result.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }

                result.Rows.Add(row);
            }

            return result;
        }

        public static System.Data.DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            System.Data.DataTable table = new System.Data.DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static System.Data.DataTable ToDataTable(this IEnumerable<IEnumerable<object>> list, IEnumerable<string> colNames = null)
        {
            System.Data.DataTable tmp = new System.Data.DataTable();
            DataColumn[] cols = null;

            if (colNames == null)
                cols = Enumerable.Range(0, list.First().Count()).Select(x => new DataColumn(x.ToString())).ToArray();
            else
                cols = colNames.Select(x => new DataColumn(x)).ToArray();

            tmp.Columns.AddRange(cols);

            foreach (IEnumerable<object> row in list)
            {
                tmp.Rows.Add(row.ToArray());
            }
            return tmp;
        }

        public static void SaveToCSV(this System.Data.DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        public static string ToCsvString(this System.Data.DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                int count = 1;
                int totalColumns = dt.Columns.Count;
                foreach (DataColumn dr in dt.Columns)
                {
                    sb.Append(dr.ColumnName);

                    if (count != totalColumns)
                    {
                        sb.Append(",");
                    }

                    count++;
                }

                sb.AppendLine();

                string value = String.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    for (int x = 0; x < totalColumns; x++)
                    {
                        value = dr[x].ToString();

                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = '"' + value.Replace("\"", "\"\"") + '"';
                        }

                        sb.Append(value);

                        if (x != (totalColumns - 1))
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                // Do something
            }

            return sb.ToString();
        }

        // function that creates a list of an object from the given data table
        public static List<T> ToList<T>(this System.Data.DataTable tbl, Dictionary<string, string> replacementDict = null) where T : new()
        {
            List<T> lst = new List<T>();

            foreach (DataRow r in tbl.Rows)
                lst.Add(r.SetItem<T>(replacementDict));

            return lst;
        }

        public static T SetItem<T>(this DataRow row, Dictionary<string, string> replacementDict = null) where T : new()
        {
            T item = new T();
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                if (p == null)
                {
                    if (replacementDict.TryGetValue(c.Caption, out string val))
                        p = item.GetType().GetProperty(val);
                }

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    if (replacementDict.ContainsKey(c.Caption))
                    {
                        SetValue(item, replacementDict[c.Caption], row[c]);
                    }
                    else
                    {
                        SetValue(item, c.Caption, row[c]);
                    }
                }
            }

            return item;
        }

        public static void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            //get the property information based on the type
            System.Reflection.PropertyInfo propertyInfo = inputObject.GetType().GetProperty(propertyName);

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = TypeHelper.IsNullableType(propertyInfo.PropertyType) ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);
        }
    }
}