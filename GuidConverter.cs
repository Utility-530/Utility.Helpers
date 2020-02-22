using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilityHelper
{
    public static class GuidConverter
    {
        public static Guid ToGUID(string input)
        {
            string text = input.TrimEnd();

            if (text.Length <= 16)
            {
                text += new string(Enumerable.Range(0, 16 - text.Length).Select(_ => ' ').ToArray());
                byte[] hash = Encoding.Default.GetBytes(text);
                return new Guid(hash);
            }
            
            return new Guid(text);
        }

        public static string FromGUID(Guid input)
        {
            byte[] buffer = input.ToByteArray();
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length); ;
        }
    }
}
