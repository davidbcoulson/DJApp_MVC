using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class PlayerCheck
    {
        public Devices device { get; set; }
        public string shufle_state { get; set; }
        public double timestamp { get; set; }
        public Context context { get; set; }
        public double progress_ms { get; set; }
        public Item item { get; set; }
        public bool is_playing { get; set; }
    }
}