using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class UserWallet : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public decimal TransactionAmount { get; set; }
        public TransactionType TransactionType { get; set; }
        public long? StoreOrderId { get; set; }
        public virtual StoreOrder StoreOrder { get; set; }
        public long? OtlobOrderId { get; set; }
        public virtual OtlobAy7agaOrder OtlobOrder { get; set; }
        public string AttachmentUrl { get; set; }
        public string TransactionWay { get; set; } // how admin paid to user
        public string TransactionId { get; set; }
    }
}