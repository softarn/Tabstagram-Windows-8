using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    class BooleanToFollowStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var res = new ResourceLoader("Resources");
            if (value == null)
                return res.GetString("Follow");

            bool val = (bool)value;
            if (val)
                return res.GetString("Unfollow");

            return res.GetString("Follow");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}