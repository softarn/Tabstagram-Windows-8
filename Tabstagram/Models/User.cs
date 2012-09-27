using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Tabstagram
{
    public class User
    {
        public string id { get; set; }
        public string username { get; set; }
        private string _full_name;
        public string full_name { 
            get
            {
                if (_full_name != null && _full_name.Trim().Equals(""))
                {
                    return username;
                }

                return _full_name;
            }
            set
            {
                _full_name = value;
            }
        }
        public string profile_picture { get; set; }
        public string bio { get; set; }
        public string website { get; set; }
        public Counts counts { get; set; }

        public static User SingleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleUser>(jsonString).data;
        }

        public static User MultipleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleUser>(jsonString).data;
        }

        private class SingleUser
        {
            public User data { get; set; }
        }

        private class MultipleUsers
        {
            public List<User> data { get; set; }
        }
    }

    public class Counts
    {
        public int media { get; set; }
        public int follows { get; set; }
        public int followed_by { get; set; }
    }
}
