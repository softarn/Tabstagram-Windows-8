using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Data;
using Tabstagram.Models;

namespace Tabstagram.Helpers
{
    class FavouriteMediaListHelper
    {
        public static MediaListViewModel GetClassFromString(string listString)
        {
            if (listString.Equals("popular"))
                return new Popular();
            if (listString.Equals("feed"))
                return new Feed();
            if (listString.Equals("selfmedia"))
                return new SelfMedia();

            if (HashTag.IsHashTag(listString))
            {
                return new HashTag(listString.Substring(1, listString.Length - 1));
            }

            throw new ArgumentException("listString must be #hashtagname, popular, selfmedia or feed");
        }

        public static bool AddMediaString(string str)
        {
            List<string> list = UserSettings.MediaStringsList;
            if (FavouriteMediaListHelper.HasMediaString(str) == false && HashTag.IsHashTag(str))
            {
                list.Add(str);
                UserSettings.MediaStringsList = list;
                return true;
            }

            return false;
        }

        public static bool HasMediaString(string str)
        {
            foreach (string str1 in UserSettings.MediaStringsList)
            {
                if (str.ToLower().Equals(str1.ToLower()))
                    return true;
            }

            return false;
        }

        public static bool DeleteMediaString(string str)
        {
            if (HashTag.IsHashTag(str) == false) return false;

            List<string> list = UserSettings.MediaStringsList;
            list.Remove(str);
            UserSettings.MediaStringsList = list;

            return true;
        }
    }
}