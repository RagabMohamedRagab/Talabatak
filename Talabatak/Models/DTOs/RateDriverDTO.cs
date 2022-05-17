using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class RateDriverDTO
    {
        public long OrderId { get; set; }
        public long DriverId { get; set; }
        public int Stars { get; set; }
        public string Review { get; set; }
    }
}