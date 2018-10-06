using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class SpotifyReturnedItem
    {
        public ExternalUrls ExternalUrls { get; set; }

        public Followers Followers { get; set; }

        public Item Item { get; set; }

        public Artists Artists { get; set; }

        public RootObject RootObject { get; set; }
    }
}