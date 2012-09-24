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
        private static string GetTagUrl(string Tag)     { return String.Format("{0}tags/{1}/media/recent?access_token={2}",  BASE_URL, Tag, AccessToken); }
        private static string GetUserMediaUrl(User u)   { return String.Format("{0}users/{1}/media/recent?access_token={2}", BASE_URL, u.id, AccessToken); }
        private static string GetUserInfoUrl(string id) { return String.Format("{0}users/{1}?access_token={2}",              BASE_URL, id, AccessToken); }
        private static string GetCommentsUrl(string id) { return String.Format("{0}media/{1}/comments?access_token={2}",     BASE_URL, id, AccessToken); }

        private static HttpClient client = GetHttpClient();

        private static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", HEADER_VALUE);

            return httpClient;
        }

        private static async Task<string> GetString(string url, Args args, int tries = 0)
        {
            if (tries > 3)
                return null;

            Task<string> task = null;
            string response = null;

            if (args != null)
            {
                url = url + args.ToString();
            }

            try
            {
                response = await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                task = GetString(url, null, tries + 1);
            }

            if (task != null)
                response = await task;

            return response;
        }

        private static async Task<MultipleMedia> LoadMediaList(string url, Args args = null)
        {
            Debug.WriteLine("Getting MediaList from: " + url);
            string response = await GetString(url, args);
            MultipleMedia mm = Media.ListFromJSON(response);
            return mm;
        }

        private static async Task<User> LoadUser(string url, Args args = null, int tries = 0)
        {
            string response = await GetString(url, args);
            User user = User.SingleFromJSON(response);
            return user;
        }

        public static async Task<List<Comment>> LoadComments(string mediaId, Args args = null)
        {
            string url = GetCommentsUrl(mediaId);
            Debug.WriteLine("Getting comments from: " + url);
            string response = await GetString(url, args);
            List<Comment> comments = Comment.ListFromJSON(response);
            return comments;
        }

        public static async Task<MultipleMedia> LoadFeed(Args args = null)
        {
            return await LoadMediaList(FEED_URL, args);
        }

        public static async Task<MultipleMedia> LoadPopular(Args args = null)
        {
            return await LoadMediaList(POPULAR_URL, args);
        }

        public static async Task<MultipleMedia> LoadHashtag(string hashtag, Args args = null)
        {
            return await LoadMediaList(GetTagUrl(hashtag), args);
        }

        public static async Task<MultipleMedia> LoadFromCustomUrl(string url, Args args = null)
        {
            return await LoadMediaList(url, args);
        }

        internal static async Task<MultipleMedia> LoadUserMedia(User user, Args args = null)
        {
            return await LoadMediaList(GetUserMediaUrl(user), args);
        }

        internal static async Task<User> LoadUserInfo(string userId, Args args = null)
        {
            return await LoadUser(GetUserInfoUrl(userId), args);
        }
    }

    public class Args : List<Tabstagram.Args.Arg>
    {
        public override string ToString()
            {
                Debug.WriteLine(this.First().type.ToString());
                string delim = "&";
                StringBuilder sb = new StringBuilder();
                foreach (Arg a in this)
                {
                    sb.Append(String.Format("{0}{1}={2}", delim, a.type.ToString().ToLower(), a.value));
                    delim = "&";
                }
                return sb.ToString();
            }

        public class Arg
        {
            public enum Type { MIN_ID, MAX_ID, MIN_TAG_ID, MAX_TAG_ID, COUNT }

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
