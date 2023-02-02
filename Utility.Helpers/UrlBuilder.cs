using System.Net;
using System.Text;

namespace Utility.Helpers
{
    //consider SqlKata (nuget)
    //Stack Overflow Aug 18 '13 at 17:58 Bjoern
    //Building URI with the http client API
    /// <summary>
    /// Helper class for creating a URI with query string parameter.
    /// </summary>
    public class UrlBuilder
    {
        private StringBuilder UrlStringBuilder { get; set; }
        private bool FirstParameter { get; set; }

        /// <summary>
        /// Creates an instance of the UriBuilder
        /// </summary>
        /// <param name="baseUrl">the base address (e.g: http://localhost:12345)</param>
        public UrlBuilder(string baseUrl)
        {
            UrlStringBuilder = new StringBuilder(baseUrl);
            FirstParameter = true;
        }

        /// <summary>
        /// Adds a new parameter to the URI
        /// </summary>
        /// <param name="key">the key </param>
        /// <param name="value">the value</param>
        /// <remarks>
        /// The value will be converted to a url valid coding.
        /// </remarks>
        public void AddParameter(string key, string value)
        {
            string urlEncodeValue = WebUtility.UrlEncode(value);

            if (FirstParameter)
            {
                UrlStringBuilder.AppendFormat("?{0}={1}", key, urlEncodeValue);
                FirstParameter = false;
            }
            else
            {
                UrlStringBuilder.AppendFormat("&{0}={1}", key, urlEncodeValue);
            }
        }

        /// <summary>
        /// Gets the URI with all previously added paraemter
        /// </summary>
        /// <returns>the complete URI as a string</returns>
        public string GetUrl()
        {
            return UrlStringBuilder.ToString();
        }
    }
}