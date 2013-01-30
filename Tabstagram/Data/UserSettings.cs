using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram.Data
{
    public class UserSettings : Settings
    {
        private static string ACCESS_TOKEN_KEY = "access_token";
        private static string ACITVE_LIST_KEY = "active_list";
        private static string MEDIA_LISTS = "media_lists";
        private static string MEDIA_LIST_CHANGED = "media_list_changed";
        private static string USER_ID_KEY = "user_id";
        private static string FOLLOW_TABSTAGRAM_KEY = "follow_tabstagram_id";
        private static string ASKED_RATING_KEY = "asked_rating";

        public static bool HasAskedForRating
        {
            get { return Get<bool>(ASKED_RATING_KEY); }
            set { Save(ASKED_RATING_KEY, value); }
        }

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

        public static bool FollowTabstagram 
        { 
            get { return Get<bool>(FOLLOW_TABSTAGRAM_KEY); }
            set { Save(FOLLOW_TABSTAGRAM_KEY, value); }        
        }

        public static bool MediaListChanged
        {
            get { return Get<bool>(MEDIA_LIST_CHANGED); }
            set { Save(MEDIA_LIST_CHANGED, value); }
        }

        public static async Task<string> RetreiveUserId()
        {
            Task<string> task = null;

            string userId = Get<string>(USER_ID_KEY);

            if (userId != null)
            {
                return userId;
            }
            else
            {
                try
                {
                    User user = await Instagram.LoadUserInfo("self");
                    Save(USER_ID_KEY, user.id);
                    return user.id;
                }
                catch (Exception)
                {
                    return "-1";
                }
            }
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
                UserSettings.MediaListChanged = true;
                Save(MEDIA_LISTS, ListToString(value));
            }
        }

        private static List<string> LoadDefaultMediaLists()
        {
            List<string> l = new List<string>();
            l.Add("feed");
            l.Add("popular");
            l.Add("selfmedia");
            l.Add("#tabstagram");
            l.Add("#instamood");
            l.Add("#winter");
            l.Add("#photooftheday");
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