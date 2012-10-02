using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram.Models
{
    public class UserSettings : Settings
    {
        private static string ACCESS_TOKEN_KEY = "access_token";
        private static string ACITVE_LIST_KEY = "active_list";
        private static string MEDIA_LISTS = "media_lists";
        private static string USER_ID_KEY = "user_id";

        public static string AccessToken 
        {
            get { return Get<string>(ACCESS_TOKEN_KEY); } 
            set { Save(ACCESS_TOKEN_KEY, value); } 
        }

        public static string ActiveList 
        { 
            get { return Get<string>(ACITVE_LIST_KEY); } 
            set { Save(ACITVE_LIST_KEY, value); } 
        }

        public string UserId
        {
            get { return Get<string>(USER_ID_KEY); }
            set { Save(USER_ID_KEY, value); }
        }
        
        public static List<string> MediaStringsList
        {
            get
            {
                List<string> l = StringToList(Get<string>(MEDIA_LISTS));
                if (l == null || l.Count < 1)
                    l = LoadDefaultMediaLists();
                return l;
            }

            set
            {
                Save(MEDIA_LISTS, ListToString(value));
            }
        }

        public static void AddMediaString(string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MediaStringsList);
            sb.Append(str + ',');

            Save(MEDIA_LISTS, sb.ToString());
        }

        private static List<string> LoadDefaultMediaLists()
        {
            List<string> l = new List<string>();
            l.Add("feed");
            l.Add("popular");
            l.Add("selfmedia");
            l.Add("#tabstagram");

            MediaStringsList = l;
            return l;
        }

        private static List<string> StringToList(string listString)
        {
            if (listString == null) return null;
            return new List<string>(listString.Split(','));
        }

        private static string ListToString(List<string> stringList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in stringList)
            {
                sb.Append(str + ",");
            }

            string listString = sb.ToString();
            return listString.Substring(0, listString.Length - 1);
        }
    }
}