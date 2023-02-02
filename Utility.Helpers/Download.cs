using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Utility.Helpers.Generic;

namespace Utility.Helpers
{
    public static class DownloadHelper
    {
        /// <summary>
        /// Downloads the file from the specified URL to the destination directory
        /// </summary>
        public static bool DownloadToFolder(string sourceUrl, string destinationFolder, bool overwrite = true)
        {
            return Download(sourceUrl, (response) => Path.Combine(destinationFolder, Path.GetFileName(response.ResponseUri.AbsolutePath)), overwrite);
        }

        /// <summary>
        /// Downloads the file from the specified URL to the destination directory
        /// </summary>
        public static void DownloadToFolderAsync(string sourceUrl, string destinationFolder, string file)
        {
            DownloadAsync(sourceUrl, Path.Combine(destinationFolder, file));
        }

        /// <summary>
        /// Downloads the file from the specified URL to the destination file address
        /// (new WebClient).DownloadFile(sourceUrl, destinationFile) does the same thing
        /// </summary>
        public static bool DownloadToFile(string sourceUrl, string destinationFile, bool overwrite = true)
        {
            return Download(sourceUrl, (a) => destinationFile, overwrite);
        }

        private static bool Download(string sourceUrl, Func<WebResponse, string> destinationFile, bool overwrite = true)
        {
            try
            {
                var request = WebRequest.CreateHttp(sourceUrl);
                var response = request.GetResponse();

                string DownloadedFile = destinationFile(response);

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
                Console.WriteLine("error downloading file" + ex.Message);
                return false;
            }

            return true;
        }

        public static async void DownloadAsync(string url, string path)
        {
            var client = new HttpClient();

            await System.Threading.Tasks.Task.Run(() => client.GetAsync(url).ContinueWith(d =>
            {
                var x = System.Threading.Tasks.Task.Run(async () =>
                {
                    using (HttpContent content = d.Result.Content)
                    // actually a System.Net.Http.StreamContent instance but you do not need to cast as the actual type does not matter in this case
                    using (var file = File.Create(path))
                    {   // get the actual content stream
                        using (var contentStream = await content.ReadAsStreamAsync())
                        {
                            await contentStream.CopyToAsync(file); // copy that stream to the file stream
                            await file.FlushAsync(); // flush back to disk before disposing
                        }
                    }
                });
            })).ContinueWith((a) => client.Dispose());
        }

        private static async System.Threading.Tasks.Task<string> GetResponse(this HttpClient client, string url, params Tuple<string, string>[] headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new System.Uri(url)))
            {
                headers.ForEach(a => request.Headers.TryAddWithoutValidation(a.Item1, a.Item2));
                //request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                //request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                //request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                //request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var decompressedStream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        return await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}