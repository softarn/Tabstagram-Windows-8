using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    public class Meta
    {
        public int code { get; set; }
    }

    public class Comment
    {
        public string created_time { get; set; }
        public string text { get; set; }
        public User from { get; set; }
        public string id { get; set; }

        public static List<Comment> ListFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleComments>(jsonString).data;
        }

        public override bool Equals(object obj)
        {
            return ((Comment)obj).id.Equals(this.id);
        }
    }

    public class MultipleComments
    {
        public Meta meta { get; set; }
        public List<Comment> data { get; set; }
    }
}
