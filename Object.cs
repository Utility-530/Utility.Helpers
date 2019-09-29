using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public static class ObjectHelper
    {


        public static double[][] ObjectsToDoubleArrayWithoutNull(IEnumerable<object> objects, params string[] excludeProperties)
        {
            var props = objects.First().GetType()
        .GetProperties()
        .Where(p => (!excludeProperties.Contains(p.Name)))
         .Where(p => { var x = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType; return x.IsNumericType(); }).ToList();

            List<double[]> lst = new List<double[]>();

            foreach (object o in objects)
            {

                var xx = props.Select(p => { var x = p.GetValue(o); return x == null ? null : (double?)Convert.ToDouble(x); });
                if (!xx.Any(cc => cc == null))
                    lst.Add(xx.Select(v => (double)v).ToArray());

            }

            return lst.ToArray();
        }


        public static double[][] ObjectsToDoubleArrayWithoutNull(System.Collections.IEnumerable objects, params string[] excludeProperties)
        {
            var e = objects.GetEnumerator();
            e.MoveNext();
            var props = e.Current.GetType()
        .GetProperties()
        .Where(p => (!excludeProperties.Contains(p.Name)))
         .Where(p => { var x = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType; return x.IsNumericType(); }).ToList();

            List<double[]> lst = new List<double[]>();

            foreach (object o in objects)
            {

                var xx = props.Select(p => { var x = p.GetValue(o); return x == null ? null : (double?)Convert.ToDouble(x); });
                if (!xx.Any(cc => cc == null))
                    lst.Add(xx.Select(v => (double)v).ToArray());

            }

            return lst.ToArray();
        }

        public static double[][] ObjectsToDoubleArrayWithoutNull(System.Collections.IEnumerable objects, out int[] indices, params string[] excludeProperties)
        {
            var e = objects.GetEnumerator();
            e.MoveNext();
            var props = e.Current.GetType()
        .GetProperties()
        .Where(p => (!excludeProperties.Contains(p.Name)))
         .Where(p => { var x = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType; return x.IsNumericType(); }).ToList();

            List<double[]> lst = new List<double[]>();
            int i = 0;
            var indicesList = new List<int>();


            foreach (object o in objects)
            {

                var xx = props.Select(p => { var x = p.GetValue(o); return x == null ? null : (double?)Convert.ToDouble(x); });
                if (!xx.Any(cc => cc == null))
                    lst.Add(xx.Select(v => (double)v).ToArray());
                else
                    indicesList.Add(i);
                i++;
            }
            indices = indicesList.ToArray();
            return lst.ToArray();
        }





        public static double[][] ObjectsToDoubleArrayWithoutNullProperties(System.Collections.IEnumerable objects, IList<PropertyInfo> props, out IList<KeyValuePair<string, int>> propsmissing)
        {


            List<double?[]> lst = new List<double?[]>();
            int i = 0;
            var indicesList = new List<int>();
            foreach (object o in objects)
            {
                var xx = props.Select(p => { var x = p.GetValue(o); return x == null ? null : (double?)Convert.ToDouble(x); });

                lst.Add(xx.ToArray());

            }
            propsmissing = new List<KeyValuePair<string, int>>();


            for (int j = 0; j < props.Count; j++)
            {
                for (int k = 0; k < lst.Count; k++)
                {
                    if (lst[k][j] == null && !propsmissing.Select(_ => _.Key).Contains(props[j].Name))
                        propsmissing.Add(new KeyValuePair<string, int>(props[j].Name, j));

                }
            }


            return lst.ToArray().ToDouble(propsmissing.Select(_ => _.Value).ToArray());

        }

        public static double[][] ObjectsToDoubleArrayWithoutNullProperties<T>(ICollection<T> objects, IList<PropertyInfo> props, out IList<KeyValuePair<string, int>> propsmissing)
        {

            List<double?[]> lst = new List<double?[]>();


            foreach (object o in objects)
            {
                var xx = props.Select(p => { var x = p.GetValue(o); return x == null ? null : (double?)Convert.ToDouble(x); });
                lst.Add(xx.ToArray());

            }

            propsmissing = new List<KeyValuePair<string, int>>();


            for (int j = 0; j < props.Count; j++)
            {
                for (int k = 0; k < lst.Count; k++)
                {
                    if (lst[k][j] == null && !propsmissing.Select(_ => _.Key).Contains(props[j].Name))
                        propsmissing.Add(new KeyValuePair<string, int>(props[j].Name, j));

                }
            }

            return lst.ToArray().ToDouble(propsmissing.Select(_ => _.Value).ToArray());


        }




        public static double[][] ToDouble(this double?[][] arr, int[] propsmissing)
        {
            var conv = new Converter<double?, double>(_ => (double)_);

            var kx = arr.RemoveColumns(propsmissing);

            var xy = new double[kx.Length][];
            for (int k = 0; k < kx.Length; k++)
            {
                xy[k] = Array.ConvertAll<double?, double>(kx[k], conv);
            }
            return xy;
        }




        public static IList<PropertyInfo> GetNumericProperties<T>(params string[] excludeProperties) =>
            typeof(T)
        .GetProperties()
        .Where(p => (!excludeProperties.Contains(p.Name)))
         .Where(p => { var x = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType; return x.IsNumericType(); }).ToList();



        public static IEnumerable<PropertyInfo> GetDateTimeProperties<T>(params string[] excludeProperties) =>

                 typeof(T)
        .GetProperties()
        .Where(p => !excludeProperties.Contains(p.Name))
        .FilterTypes(_ => IsDateTimeType(_, out DateTime val));





        public static IEnumerable<PropertyInfo> FilterTypes(this IEnumerable<PropertyInfo> props, Func<Type, bool> filter) =>
            props
             .Where(p => { var x = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType; return filter(x); });





        public static IEnumerable<PropertyInfo> GetNumericProperties(object obj, params string[] excludeProperties) =>

             obj.GetType()
        .GetProperties()
        .Where(p => !excludeProperties.Contains(p.Name))
        .FilterTypes(PropertyHelper.IsNumericType);



        public static bool IsAnyNullOrEmpty(object myObject)
        {
            return IsNullOrEmpty(myObject).Any(_ => _ == true);

        }

        public static IEnumerable<bool> IsNullOrEmpty(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                yield return pi.GetValue(myObject) == null;

            }

        }




        public static bool IsDateTimeType(object value)
        {

            var dateValue = default(DateTime);
            var type = value.GetType();
            if (type == typeof(DateTime))
            {
                dateValue = (DateTime)value;
                return true;
            }
            else if (type == typeof(string))
            {
                return DateTimeHelper.TryParseDate1((string)value, out dateValue) || DateTimeHelper.TryParseDate2((string)value, out dateValue);
            }
            else
            {
                return false;
            }
        }


        public static bool IsDateTimeType(object value, out DateTime dateValue)
        {

            dateValue = default(DateTime);
            var type = value.GetType();
            if (type == typeof(DateTime))
            {
                dateValue = (DateTime)value;
                return true;

            }
            else if (type == typeof(string))
            {
                return DateTimeHelper.TryParseDate1((string)value, out dateValue) || DateTimeHelper.TryParseDate2((string)value, out dateValue);
                //    Console.WriteLine("Converted '{0}' to {1}.", dateString, dateValue);
                //else
                //    Console.WriteLine("Unable to convert '{0}' to a date.", dateString);
            }
            else
            {
                return false;
            }
        }



    }
}
