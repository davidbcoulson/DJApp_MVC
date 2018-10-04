using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class Item
    {
        public Album album { get; set; }
        public List<Artist> artists { get; set; }
        public List<string> available_markets { get; set; }
        public string disc_number { get; set; }
        public double duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalIds external_ids { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int populatrity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
}