using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.DTOs
{
    public class ProductDTO
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMultipleSize { get; set; }
        public string SingleOriginalPrice { get; set; }
        public string SingleOfferPrice { get; set; }
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsFavorite { get; set; }
        public string Images { get; set; } 
        public ICollection<ProductSizeDTO> Sizes { get; set; } 
       
    }
}