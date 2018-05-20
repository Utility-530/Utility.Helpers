using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using LumenWorks.Framework.IO.Csv;



namespace UtilityHelper
{

    public static class DataTableExtension
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




        public static DataTable ToDataTable(this IEnumerable<IEnumerable<object>> list, IEnumerable<string> colNames = null)
        {
            DataTable tmp = new DataTable();
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



        public static void SaveToCSV(this DataTable dtDataTable, string strFilePath)
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





        public static string ToCsv(this DataTable dt)
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
    }
}
