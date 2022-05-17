using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class PaymentHistory : BaseModel
    {
        public bool IsStoreOrder { get; set; }
        public long? StoreOrderId { get; set; }
        public virtual StoreOrder StoreOrder { get; set; }
        public long? OtlobAy7agaOrderId { get; set; }
        public virtual OtlobAy7agaOrder OtlobAy7agaOrder { get; set; }
        public string TransactionId { get; set; }
    }
}