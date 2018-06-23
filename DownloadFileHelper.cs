
using System;
using System.IO;
using System.Net;


namespace UtilityHelper
{
    /// <summary>
    /// Downloads a file from a URL to a destination directory 
    /// or file path.
    /// </summary>
    public class DownloadFileHelper
    {

        /// <summary>
        /// Downloads the file from the specified URL to 
        /// the destination directory
        /// </summary>
        public static bool DownloadToFolder(string SourceUrl, string DestinationFolder,bool Overwrite = true)
        {

            try
            {
                var request = WebRequest.CreateHttp(SourceUrl);
                var response = request.GetResponse();

                string      DownloadedFile = Path.Combine(DestinationFolder, Path.GetFileName(response.ResponseUri.AbsolutePath));

                var bufferSize = 4096;

                using (var webStream = response.GetResponseStream())
                using (var localStream = File.Create(DownloadedFile))
                {
                    var buffer = new byte[bufferSize];
                    var read = 0;
                    while ((read = webStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        localStream.Write(buffer, 0, read);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Downloads the file from the specified URL to 
        /// the destination directory
        /// </summary>
        public static bool DownloadToFile(string SourceUrl, string DestinationFile, bool Overwrite = true)
        {

            try
            {
                var request = WebRequest.CreateHttp(SourceUrl);
                var response = request.GetResponse();

                string DownloadedFile = DestinationFile;
             
                var bufferSize = 4096;

                using (var webStream = response.GetResponseStream())
                using (var localStream = File.Create(DownloadedFile))
                {
                    var buffer = new byte[bufferSize];
                    var read = 0;
                    while ((read = webStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        localStream.Write(buffer, 0, read);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }
    }
}
