using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class Album
    {
        public string album_type { get; set; }
        public List<Artist> artists { get; set; }
        public ExternalUrls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
}