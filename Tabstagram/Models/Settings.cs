using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram.Models
{
    public class Settings
    {
        private static Windows.Storage.ApplicationDataContainer localSettings { get { return Windows.Storage.ApplicationData.Current.LocalSettings; } }

        protected static void Save<T>(string key, T value)
        {
            localSettings.Values[key] = value;
        }

        protected static T Get<T>(string key)
        {
            return (T)localSettings.Values[key];
        }

        public async static void ClearSettings()
        {
            await Windows.Storage.ApplicationData.Current.ClearAsync();
        }
    }
}
