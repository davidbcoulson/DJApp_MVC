using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DJApp_MVC.Models
{
    public class LocationDevices
    {
        public string LocationId { get; set; }
        public List<Devices> devices { get; set; }
    }
}