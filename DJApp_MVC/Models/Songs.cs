using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class Songs
    {
        public string SongTitle { get; set; }
        public string SongAlbum { get; set; }
        public string SongSpotifyId { get; set; }
        public string SongImageUrl { get; set; }
        public string SongArtist { get; set; }
    }
}