using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class StoreReviewDTO
    {
        public long StoreId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Rate { get; set; }
        public string Review { get; set; }
        public long? StoreOrderId { get; set; }

    }
}