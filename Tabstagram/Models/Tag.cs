using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;

namespace Tabstagram
{
    public class Tag
    {
        public int media_count { get; set; }
        public string name { get; set; }

        public static MultipleTags MultipleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleTags>(jsonString);
        }
    }

    public class SingleTag
    {
        public Tag data { get; set; }
    }

    public class MultipleTags
    {
        public Pagination pagination { get; set; }
        public IEnumerable<Tag> data { get; set; }
    }
}
