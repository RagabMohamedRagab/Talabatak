using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class StoreOrderItem : BaseModel
    {
        public long? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public long? SizeId { get; set; }
        public virtual ProductSize Size { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public long OrderId { get; set; }
        public virtual StoreOrder Order { get; set; }
    }
}