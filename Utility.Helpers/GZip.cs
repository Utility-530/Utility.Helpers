using System.IO;
using System.IO.Compression;

namespace Utility.Helpers
{
    public static class GZipHelper
    {
        public static void Extract(string infile, string outfile)
        {
            using (FileStream outStream = new FileStream(outfile, FileMode.Create, FileAccess.Write))
            {
                Extract(infile, outStream);
            }
        }

        public static string Extract(string infile)
        {
            using (var outStream = new MemoryStream())
            {
                Extract(infile, outStream);
                using (var reader = new StreamReader(outStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static void Extract(string infile, Stream outStream)
        {
            using (FileStream fInStream = new FileStream(infile, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream zipStream = new GZipStream(fInStream, CompressionMode.Decompress))
                {
                    byte[] tempBytes = new byte[4096];
                    int i;
                    while ((i = zipStream.Read(tempBytes, 0, tempBytes.Length)) != 0)
                    {
                        outStream.Write(tempBytes, 0, i);
                    }
                    outStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}