using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{



    public static class DataTableMapper
    {
        //    public static string connectionString = ConfigurationManager.ConnectionStrings["YourWebConfigConnection"].ConnectionString;



        // function that creates a list of an object from the given data table
        public static List<T> ToList<T>(this DataTable tbl, Dictionary<string, string> replacementDict = null) where T : new()
        {
            List<T> lst = new List<T>();

            foreach (DataRow r in tbl.Rows)            
                lst.Add(r.SetItem<T>( replacementDict));
            

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
            var targetType = IsNullableType(propertyInfo.PropertyType) ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);

        }


        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

    }
}
