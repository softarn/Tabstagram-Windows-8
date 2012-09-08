using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    class Instagram
    {

        private static string token = "48448621.f59def8.490b78f9de0041ad8d7a5534a59af67d";

        private static string BASE_URL = "https://api.instagram.com/v1/";
        private static string HEADER_VALUE = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private static string feed_url = Instagram.BASE_URL + "users/self/feed?access_token=" + token;

        public Instagram(string token)
        {
            Instagram.token = token;
        }
                        
        private HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", Instagram.HEADER_VALUE);

            return httpClient;
        }

        public async Task<List<Media>> Feed()
        {
            HttpClient client = GetHttpClient();

            string response = await client.GetStringAsync(feed_url);
            return Media.ListFromJSON(response);
        }
    }
}
