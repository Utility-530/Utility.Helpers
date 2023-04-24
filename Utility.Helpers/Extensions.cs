using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Utility.Helpers
{
    public static class Extensions
    {
        private const string _hexaChars = "0123456789ABCDEF";

        //public static string ConcatenateCollection(IEnumerable collection, string expression, string separator)
        //{
        //    return ConcatenateCollection(collection, expression, separator, null);
        //}

        public static string ConcatenateCollection(IEnumerable collection, string expression, string separator, IFormatProvider formatProvider)
        {
            if (collection == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (object o in collection)
            {
                if (i > 0)
                {
                    sb.Append(separator);
                }
                else
                {
                    i++;
                }

                if (o != null)
                {
                    //object e = ConvertUtilities.Evaluate(o, expression, typeof(string), null, formatProvider);
                    object e = DataBindingEvaluator.Eval(o, expression, formatProvider, null, false);
                    if (e != null)
                    {
                        sb.Append(e);
                    }
                }
            }
            return sb.ToString();
        }







        public static string NormalizeGuidParameter(object parameter)
        {
            const string guidParameters = "DNBPX";
            string p = $"{parameter}".ToUpperInvariant();
            if (p.Length == 0)
            {
                return guidParameters[0].ToString(CultureInfo.InvariantCulture);
            }

            char ch = guidParameters.FirstOrDefault(c => c == p[0]);
            return ch == 0 ? guidParameters[0].ToString(CultureInfo.InvariantCulture) : ch.ToString(CultureInfo.InvariantCulture);
        }




        //public static string ToHexa(byte[] bytes)
        //{
        //    if (bytes == null)
        //    {
        //        return null;
        //    }

        //    return ToHexa(bytes, 0, bytes.Length);
        //}

        //public static string ToHexa(byte[] bytes, int offset, int count)
        //{
        //    if (bytes == null)
        //    {
        //        return string.Empty;
        //    }

        //    if (offset < 0)
        //    {
        //        throw new ArgumentException(null, "offset");
        //    }

        //    if (count < 0)
        //    {
        //        throw new ArgumentException(null, "count");
        //    }

        //    if (offset >= bytes.Length)
        //    {
        //        return string.Empty;
        //    }

        //    count = Math.Min(count, bytes.Length - offset);

        //    StringBuilder sb = new StringBuilder(count * 2);
        //    for (int i = offset; i < offset + count; i++)
        //    {
        //        sb.Append(_hexaChars[bytes[i] / 16]);
        //        sb.Append(_hexaChars[bytes[i] % 16]);
        //    }
        //    return sb.ToString();
        //}

    }
}