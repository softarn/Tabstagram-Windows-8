using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram.Models
{
    public class Meta
    {
        public int code { get; set; }
    }

    public class Data
    {
        public string outgoing_status { get; set; }
        public bool target_user_is_private { get; set; }
        public string incoming_status { get; set; }
        public bool outgoing_follow
        {
            get
            {
                return outgoing_status.ToLower().Equals("follows");
            }
        }

    }

    public class Relationship
    {
        public enum Type
        {
            FOLLOW = 0,
            UNFOLLOW = 1
        }

        public Meta meta { get; set; }
        public Data data { get; set; }

        internal static Relationship FromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<Relationship>(jsonString);
        }
    }
}
