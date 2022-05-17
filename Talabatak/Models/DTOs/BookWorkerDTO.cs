using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class BookWorkerDTO
    {
        public long JobWorkerId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string AddressInDetails { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public string Apartment { get; set; }
        public string Description { get; set; }
        public List<string> Images { get; set; }
    }
}