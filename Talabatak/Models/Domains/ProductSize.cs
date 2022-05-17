using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class ProductSize : BaseModel
    {
        public long ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string SizeAr { get; set; }
        public string SizeEn { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? OfferPrice { get; set; }
        public virtual ICollection<StoreOrderItem> StoreOrderItems { get; set; }
    }
}