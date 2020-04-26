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
        public static Guid ToGUID(long input)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(input).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static Guid ToGUID(int input)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(input).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static string FromGUID(Guid input)
        {
            byte[] buffer = input.ToByteArray();
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length); ;
        }

        public static long FromGUIDToLong(Guid input)
        {
            byte[] buffer = input.ToByteArray();

            long l = BitConverter.ToInt64(buffer, 0);
            return l;
        }

        public static int FromGUIDToInt(Guid input)
        {
            byte[] buffer = input.ToByteArray();

            int i = BitConverter.ToInt32(buffer, 0);
            return i;
        }


    }
}
