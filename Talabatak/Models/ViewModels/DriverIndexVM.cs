using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.ViewModels
{
    public class DriverIndexVM  
    {
        public Driver Driv { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
    }
}