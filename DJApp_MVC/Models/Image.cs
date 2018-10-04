using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class Image
    {
        public int? Height { get; set; }

        public string Url { get; set; }

        public int? Width { get; set; }
    }
}