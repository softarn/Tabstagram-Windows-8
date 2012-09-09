using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    class Instagram
    {

        public static string access_token = "";

        private static string BASE_URL = "https://api.instagram.com/v1/";
        private static string HEADER_VALUE = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private static string FEED_URL { get { return BASE_URL + "users/self/feed?access_token=" + access_token; } }
                        
        private static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", HEADER_VALUE);

            return httpClient;
        }

        public static async Task<List<Media>> Feed()
        {
            Debug.WriteLine(FEED_URL);
            HttpClient client = GetHttpClient();
            string response = await client.GetStringAsync(FEED_URL);
            return Media.ListFromJSON(response);
        }

    }
}
