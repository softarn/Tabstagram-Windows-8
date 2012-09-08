using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    public class Location
    {
        public string id { get; set; }
        public string name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

        public static Location SingleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleLocation>(jsonString).data;
        }

        public static List<Location> ArrayFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleLocations>(jsonString).data;
        }
    }

    public class SingleLocation
    {
        public Location data { get; set; }
    }

    public class MultipleLocations
    {
        public List<Location> data { get; set; }
    }
}
