using System;
//using ICSharpCode.SharpZipLib.Zip;
using System.Net;
using System.IO;
using System.IO.Compression;


namespace UtilityHelper
{
    public static class GZip
    {
        public static void Extract(string infile, string outfile)
        {
            using (FileStream fInStream = new FileStream(infile, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream zipStream = new GZipStream(fInStream, CompressionMode.Decompress))
                {
                    using (FileStream fOutStream = new FileStream(outfile,
                    FileMode.Create, FileAccess.Write))
                    {
                        byte[] tempBytes = new byte[4096];
                        int i;
                        while ((i = zipStream.Read(tempBytes, 0, tempBytes.Length)) != 0)
                        {
                            fOutStream.Write(tempBytes, 0, i);
                        }
                    }
                }
            }
        }

        public static string Extract(string infile)
        {
            using (FileStream fInStream = new FileStream(infile, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream zipStream = new GZipStream(fInStream, CompressionMode.Decompress))
                {
                    using (MemoryStream fOutStream = new MemoryStream())
                    {
                        byte[] tempBytes = new byte[4096];
                        int i;
                        while ((i = zipStream.Read(tempBytes, 0, tempBytes.Length)) != 0)
                        {
                            fOutStream.Write(tempBytes, 0, i);
                        }
                        using (var reader = new StreamReader(fOutStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}
