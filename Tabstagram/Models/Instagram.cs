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
        public static string AccessToken { get; set; }

        private static string BASE_URL = "https://api.instagram.com/v1/";
        private static string HEADER_VALUE = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private static string FEED_URL { get { return BASE_URL + "users/self/feed?access_token=" + AccessToken; } }
        private static string POPULAR_URL { get { return BASE_URL + "media/popular?access_token=" + AccessToken; } }
        private static string GetTagUrl(string Tag)
        {
            return String.Format("{0}tags/{1}/media/recent?access_token={2}", BASE_URL, Tag, AccessToken);
        }
                        
        private static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", HEADER_VALUE);

            return httpClient;
        }

        private static async Task<List<Media>> LoadMediaList(string url, List<Arg> args = null)
        {
            HttpClient client = GetHttpClient();
            Debug.WriteLine("Getting MediaList from: " + url);
            string response = await client.GetStringAsync(url);
            List<Media> list = Media.ListFromJSON(response);
            return list;
        }

        public static async Task<List<Media>> LoadFeed(List<Arg> args = null)
        {
            return await LoadMediaList(FEED_URL, args);
        }

        public static async Task<List<Media>> LoadPopular(List<Arg> args = null)
        {
            return await LoadMediaList(POPULAR_URL, args);
        }

        public class Arg
        {
            public enum Type { MIN_ID, MAX_ID }

            public Type type { get; set; }
            public string value { get; set; }

            public Arg(Type t, string v) 
            {
                type = t;
                value = v;
            }
        }
    }
}
