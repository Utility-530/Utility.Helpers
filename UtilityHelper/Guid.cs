using System;
using System.Linq;
using System.Text;

namespace UtilityHelper
{
    public static class GuidHelper
    {
        public static Guid ToGuid(this string input)
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

        public static Guid ToGuid(this long input)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(input).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static Guid ToGuid(this int input)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(input).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static string ToString(this Guid input)
        {
            byte[] buffer = input.ToByteArray();
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length - Trim());

            int Trim()
            {
                return buffer
                    .Reverse()
                    .Aggregate((0, true), (a, b) => a.Item2 && b == 32 ? (a.Item1 + 1, true) : (a.Item1, false)).Item1;
            }
        }

        public static long ToLong(this Guid input)
        {
            byte[] buffer = input.ToByteArray();

            long l = BitConverter.ToInt64(buffer, 0);
            return l;
        }

        public static int ToInt(this Guid input)
        {
            byte[] buffer = input.ToByteArray();

            int i = BitConverter.ToInt32(buffer, 0);
            return i;
        }

        public static Guid ToGuid(this DateTime date)
        {
            var bytes = BitConverter.GetBytes(date.Ticks);
            Array.Resize(ref bytes, 16);
            return new Guid(bytes);
        }

        public static DateTime ToDateTime(this Guid guid)
        {
            var dateBytes = guid.ToByteArray();
            Array.Resize(ref dateBytes, 8);
            return new DateTime(BitConverter.ToInt64(dateBytes, 0));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1383030/how-to-combine-two-guid-values
        /// </summary>
        public static Guid Merge(this Guid guidA, params Guid[] guidBs)
        {
            var aba = guidA.ToByteArray();
            var cba = new byte[aba.Length];
            Array.Copy(aba, cba, aba.Length);

            foreach (var byteArray in guidBs.Select(a => a.ToByteArray()))
            {
                cba = MergeTwo(byteArray, cba);
            }

            return new Guid(cba);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1383030/how-to-combine-two-guid-values
        /// </summary>
        public static byte[] MergeTwo(byte[] aba, byte[] bba)
        {
            var cba = new byte[aba.Length];

            for (var ix = 0; ix < cba.Length; ix++)
            {
                cba[ix] = (byte)(aba[ix] ^ bba[ix]);
            }

            return cba;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1383030/how-to-combine-two-guid-values
        /// </summary>
        public static Guid MergeTwo(Guid guidA, Guid guidB)
        {
            var aba = guidA.ToByteArray();
            var bba = guidB.ToByteArray();
            var cba = new byte[aba.Length];

            for (var ix = 0; ix < cba.Length; ix++)
            {
                cba[ix] = (byte)(aba[ix] ^ bba[ix]);
            }

            return new Guid(cba);
        }
    }
}