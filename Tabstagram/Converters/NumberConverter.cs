using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            CultureInfo ci;
            if(language != null)
                ci = new CultureInfo(language);
            else 
                ci = CultureInfo.InvariantCulture;

            return ((int)value).ToString("N0", ci);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}