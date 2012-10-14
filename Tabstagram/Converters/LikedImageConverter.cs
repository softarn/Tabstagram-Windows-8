using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    class LikedImageConverter  : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool val = (bool) value;

            if(val) {
                return  new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:/Assets/HeartIcon.png")); ;
            } else {
                return new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:/Assets/HeartIcon_white.png")); ;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
