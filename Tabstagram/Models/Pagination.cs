using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram.Models
{
    public class Pagination
    {
        public string next_max_tag_id { get; set; }
        public string next_max_id { get; set; }
        public string next_min_id { get; set; }
        public string min_tag_id { get; set; }
        public string next_url { get; set; }
    }

}
