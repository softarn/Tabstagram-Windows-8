using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val = double.Parse((string)value);
            double now = (DateTime.Now - (new DateTime(1970, 1, 1))).TotalSeconds;

            TimeSpan ts = TimeSpan.FromSeconds(now - val);
            if (ts.Days > 0)
                return String.Format("Uploaded {0} {1} ago", ts.Days, StripLastIfOne("days", ts.Days));
            else if(ts.Hours > 0)
                return String.Format("Uploaded {0} {1} ago", ts.Hours, StripLastIfOne("hours", ts.Hours));
            else if(ts.Minutes > 0)
                return String.Format("Uploaded {0} {1} ago", ts.Minutes, StripLastIfOne("minutes", ts.Minutes));
            else
                return String.Format("Uploaded {0} {1} ago", ts.Seconds, StripLastIfOne("seconds", ts.Seconds));
        }

        private string StripLastIfOne(string str, int i)
        {
            return i == 1 ? str.Substring(0, str.Length - 1) : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}