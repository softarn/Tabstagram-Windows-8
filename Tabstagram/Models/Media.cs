using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;

namespace Tabstagram
{

    public class Comments
    {
        public List<Comment> data { get; set; }
        public int count { get; set; }
    }

    public class Likes
    {
        public int count { get; set; }
        public List<User> data { get; set; }
    }

    public class LowResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class StandardResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Images
    {
        public LowResolution low_resolution { get; set; }
        public Thumbnail thumbnail { get; set; }
        public StandardResolution standard_resolution { get; set; }
    }

    public class Caption
    {
        public string created_time { get; set; }
        public string text { get; set; }
        public User from { get; set; }
        public string id { get; set; }
    }

    public class Media
    {
        public string type { get; set; }
        public string filter { get; set; }
        public List<object> tags { get; set; }
        public Comments comments { get; set; }
        public Caption caption { get; set; }
        public Likes likes { get; set; }
        public string link { get; set; }
        public User user { get; set; }
        public bool user_has_liked { get; set; }
        public string created_time { get; set; }
        public Images images { get; set; }
        public string id { get; set; }
        public Location location { get; set; }

        public static Media SingleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleMedia>(jsonString).data;
        }

        public static MultipleMedia ListFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleMedia>(jsonString);
        }

        public override string ToString()
        {
            return link;
        }
    }

    public class SingleMedia
    {
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
        public Media data { get; set; }
    }

    public class MultipleMedia
    {
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
        public List<Media> data { get; set; }
    }
}
