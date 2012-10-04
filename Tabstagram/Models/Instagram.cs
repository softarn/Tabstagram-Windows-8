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

        private static string FEED_URL { get { return BASE_URL + "users/self/feed"; } }
        private static string POPULAR_URL { get { return BASE_URL + "media/popular"; } }

        private static string GetTagUrl(string Tag) { return String.Format("{0}tags/{1}/media/recent",  BASE_URL, Tag); }
        private static string GetUserMediaUrl(User u) { return String.Format("{0}users/{1}/media/recent", BASE_URL, u.id); }
        private static string GetSelfMediaUrl() { return String.Format("{0}users/self/media/recent", BASE_URL); }
        private static string GetUserInfoUrl(string id) { return String.Format("{0}users/{1}", BASE_URL, id); }
        private static string GetCommentsUrl(string id) { return String.Format("{0}media/{1}/comments", BASE_URL, id); }
        private static string GetDeleteCommentsUrl(string mediaId, string commentId) { return String.Format("{0}media/{1}/comments/{2}", BASE_URL, mediaId, commentId); }
        private static string GetLikeUrl(string id) { return String.Format("{0}media/{1}/likes", BASE_URL, id); }

        private static HttpClient client = GetHttpClient();

        private static HttpClient GetHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Add("user-agent", HEADER_VALUE);

            return httpClient;
        }

        private static Args SafeAddAccessToken(Args args)
        {
            args = Args.SafeAdd(args, new Arg(Arg.Type.ACCESS_TOKEN, AccessToken));
            return args;
        }

        private static async Task<HttpResponseMessage> Delete(string url, Args args = null, int tries = 0)
        {
            Task<HttpResponseMessage> task = null;
            HttpResponseMessage response = null;

            if (args != null)
                url = url + args.ToGetString();

            try { response = await client.DeleteAsync(url); }
            catch (Exception e)
            {
                if (tries > 3)
                    throw new HttpRequestException("Failed to do Delete", e.InnerException);
                else
                    task = Delete(url, null, tries + 1);
            }

            if (task != null)
                response = await task;

            return response;
        }

        private static async Task<HttpResponseMessage> Post(string url, Args args = null, int tries = 0)
        {
            Task<HttpResponseMessage> task = null;
            HttpResponseMessage response = null;
            String argsString = "";

            if (args != null)
                argsString = args.ToPostString();

            try { response = await client.PostAsync(url, new StringContent(argsString)); }
            catch (Exception e)
            {
                if (tries > 3)
                    throw new HttpRequestException("Failed to do Post", e.InnerException);
                else
                    task = Post(url, null, tries + 1);
            }

            if (task != null)
                response = await task;
            
            return response;
        }

        private static async Task<string> Get(string url, Args args, int tries = 0)
        {
            Task<string> task = null;
            string response = null;

            if (args != null)
                url = url + args.ToGetString();

            Debug.WriteLine("Get: " + url);

            try { response = await client.GetStringAsync(url); }
            catch (Exception e)
            {
                if (tries > 3)
                    throw new HttpRequestException("Failed to do Get", e.InnerException);
                else
                    task = Get(url, null, tries + 1);
            }

            if (task != null)
                response = await task;

            return response;
        }

        private static async Task<MultipleMedia> LoadMediaList(string url, Args args = null)
        {
            args = SafeAddAccessToken(args);
            string response = await Get(url, args);
            MultipleMedia mm = Media.ListFromJSON(response);
            return mm;
        }

        private static async Task<User> LoadUser(string url, Args args = null, int tries = 0)
        {
            args = SafeAddAccessToken(args);
            string response = await Get(url, args);
            User user = User.SingleFromJSON(response);
            return user;
        }

        public static async Task<List<Comment>> LoadComments(string mediaId, Args args = null)
        {
            args = SafeAddAccessToken(args);
            string url = GetCommentsUrl(mediaId);
            Debug.WriteLine("Getting comments from: " + url);
            string response = await Get(url, args);
            List<Comment> comments = Comment.ListFromJSON(response);
            return comments;
        }

        public static async Task<bool> CommentMedia(string mediaId, string comment)
        {
            Args args = new Args(new Arg(Arg.Type.TEXT, comment));
            args = SafeAddAccessToken(args);
            string url = GetCommentsUrl(mediaId);
            Debug.WriteLine("Commenting on: " + mediaId);
            HttpResponseMessage response = await Post(url, args);
            return response.IsSuccessStatusCode;
        }

        internal static async Task<bool> DeleteComment(string mediaId, string commentId)
        {
            Args args = new Args(new Arg(Arg.Type.ACCESS_TOKEN, AccessToken));
            string url = GetDeleteCommentsUrl(mediaId, commentId);
            HttpResponseMessage response = await Delete(url, args);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> Like(string mediaId, Args args = null)
        {
            args = SafeAddAccessToken(args);
            string url = GetLikeUrl(mediaId);
            Debug.WriteLine("Likeing media: " + mediaId);
            HttpResponseMessage response = await Post(url, args);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> Unlike(string mediaId, Args args = null)
        {
            args = SafeAddAccessToken(args);
            string url = GetLikeUrl(mediaId);
            Debug.WriteLine("Likeing media: " + mediaId);
            HttpResponseMessage response = await Delete(url, args);
            return response.IsSuccessStatusCode;
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
            string response = await Get(url, args);
            MultipleMedia mm = Media.ListFromJSON(response);
            return mm;
        }

        internal static async Task<MultipleMedia> LoadUserMedia(User user, Args args = null)
        {
            return await LoadMediaList(GetUserMediaUrl(user), args);
        }

        internal static async Task<MultipleMedia> LoadSelfMedia(Args args = null)
        {
            return await LoadMediaList(GetSelfMediaUrl(), args);
        }

        internal static async Task<User> LoadUserInfo(string userId, Args args = null)
        {
            return await LoadUser(GetUserInfoUrl(userId), args);
        }
    }

    public class Args : List<Arg>
    {
        public Args() { }

        public Args(Arg arg)
        {
            Add(arg);
        }

        public string ToGetString()
        {
            Debug.WriteLine(this.First().type.ToString());
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            string delim = "";
            foreach (Arg a in this)
            {
                sb.Append(String.Format("{0}{1}={2}", delim, a.type.ToString().ToLower(), a.value));
                delim = "&";
            }
            return sb.ToString();
        }

        public string ToPostString()
        {
            string str = this.ToGetString();
            return str.Substring(1);
        }

        public static Args SafeAdd(Args args, Arg arg)
        {
            if (args == null)
                args = new Args();

            args.Add(arg);
            return args;
        }
    }

    public class Arg
    {
        public enum Type { MIN_ID, MAX_ID, MIN_TAG_ID, MAX_TAG_ID, COUNT, TEXT, ACCESS_TOKEN }

        public Type type { get; set; }
        public string value { get; set; }

        public Arg(Type t, string v)
        {
            type = t;
            value = v;
        }
    }
}
