using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class SearchResultItem
    {
        public string ArtistName { get; set; }

        public string ArtistSpotifyId { get; set; }

        public string ImageUrl { get; set; }

        public List<object> Image { get; set; }
    }
}