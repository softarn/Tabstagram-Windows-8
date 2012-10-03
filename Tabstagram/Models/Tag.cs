using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    public class Tag
    {
        public int media_count { get; set; }
        public string name { get; set; }
    }

    public class SingleTag
    {
        public Tag data { get; set; }
    }
}
