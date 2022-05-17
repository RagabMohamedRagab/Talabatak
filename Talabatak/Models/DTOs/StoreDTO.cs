using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class StoreDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
        public string Rate { get; set; }
        public DistanceTextDto Distance { get; set; }
    }
}