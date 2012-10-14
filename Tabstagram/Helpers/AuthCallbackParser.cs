using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    class AuthCallbackParser
    {
        private string responseUrl;
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public AuthCallbackParser(string responseUrl)
        {
            this.responseUrl = responseUrl;

            Parse();
        }

        private void Parse()
        {
            string tokenPart = responseUrl.Substring(responseUrl.IndexOf("#") + 1);

            string[] parms = tokenPart.Split('&');
            foreach (string param in parms)
            {
                string name = param.Split('=')[0];
                string value = param.Split('=')[1];
                dict.Add(name, value);
            }
        }

        public string GetAccessToken()
        {
            return dict["access_token"];
        }
    }
}
