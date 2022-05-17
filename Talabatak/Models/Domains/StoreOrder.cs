using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class StoreOrder : BaseModel
    {
        public StoreOrder()
        {
            IsPaid = false;
        }
        public string PaymentUniqueKey { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Code { get; set; }
        public double StoreProfit { get; set; }
        public double DeliveryProfit { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsRefundRequired { get; set; }
        public bool IsPaid { get; set; }
        public StoreOrderStatus Status { get; set; }
        public bool IsDeliveryFeesUpdated { get; set; }
        public long? UserAddressId { get; set; }
        public virtual UserAddress UserAddress { get; set; }
        public long? DriverId { get; set; }
        public virtual Driver Driver { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<StoreOrderItem> Items { get; set; }
        public virtual ICollection<StoreOrderDriver> Drivers { get; set; }
        public virtual ICollection<DriverReview> DriverReviews { get; set; }
        public virtual ICollection<StoreReview> StoreReviews { get; set; }
        public virtual ICollection<StoreOrderChat> Chats { get; set; }
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
    }
}