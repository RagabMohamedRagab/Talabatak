using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class ProductSizeDTO
    {
        public long SizeId { get; set; }
        public string Name { get; set; }
        public string OriginalPrice { get; set; }
        public string OfferPrice { get; set; }
    }
}