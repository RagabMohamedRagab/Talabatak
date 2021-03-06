using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class StoreReview : BaseModel
    {
        public long StoreId { get; set; }
        public virtual Store Store { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Rate { get; set; }
        public string Review { get; set; }
        public long? StoreOrderId { get; set; }
        public virtual StoreOrder StoreOrder { get; set; }
    }
}