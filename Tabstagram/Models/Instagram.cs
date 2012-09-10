using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    public class MediaListType
    {
        public enum ListType { Feed, Popular, Tag }

        public ListType Type { get; set; }
        public string Tag { get; set; }

        public MediaListType(ListType type, string tag = "")
        {
            this.Type = type;
            this.Tag = tag;
        }

        public string GetName()
        {
            switch (this.Type)
            {
                case MediaListType.ListType.Feed: return "Feed";
                case MediaListType.ListType.Popular: return "Popular";
                case MediaListType.ListType.Tag: return "#" + this.Tag;
            }

            return "Error";
        }
    }

    class Instagram
    {
        public static string access_token = "";

        private static string BASE_URL = "https://api.instagram.com/v1/";
        private static string HEADER_VALUE = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private static string FEED_URL { get { return BASE_URL + "users/self/feed?access_token=" + access_token; } }
        private static string POPULAR_URL { get { return BASE_URL + "media/popular?access_token=" + access_token; } }
                        
        private static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", HEADER_VALUE);

            return httpClient;
        }

        private static string GetTagUrl(string Tag)
        {
            return String.Format("{0}tags/{1}/media/recent?access_token={2}", BASE_URL, Tag, access_token);
        }

        public static async Task<List<Media>> LoadMediaList(MediaListType mlt)
        {
            HttpClient client = GetHttpClient();
            string url = "";
            switch (mlt.Type) {
                case MediaListType.ListType.Feed: url = FEED_URL; break;
                case MediaListType.ListType.Popular: url = POPULAR_URL; break;
                case MediaListType.ListType.Tag: url = GetTagUrl(mlt.Tag); break;
            }

            Debug.WriteLine("Getting MediaList from: " + url);
            string response = await client.GetStringAsync(url);
            List<Media> list = Media.ListFromJSON(response);
            return list;
        }
    }
}
