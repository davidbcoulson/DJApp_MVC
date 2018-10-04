using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class Search
    {
        public string UserName { get; set; }

        public string ArtistQuery { get; set; }

        public List<SearchResultItem> Results { get; set; }
    }
}