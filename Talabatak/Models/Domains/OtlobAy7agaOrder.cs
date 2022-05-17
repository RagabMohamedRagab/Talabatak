using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class OtlobAy7agaOrder : BaseModel
    {
        public long CityId { get; set; }
        public virtual City City { get; set; }
        public OtlobAy7agaOrderStatus OrderStatus { get; set; }
        public string Code { get; set; }
        public string Details { get; set; }
        public string ImageUrl { get; set; }
        public double DeliveryProfit { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public bool IsPaid { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long? DriverId { get; set; }
        public virtual Driver Driver { get; set; }
        public int EstimatedDeliverDistance { get; set; } // in meters
        public int EstimatedDeliverTimeInSeconds { get; set; }
        public long? UserAddressId { get; set; }
        public virtual UserAddress UserAddress { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public virtual ICollection<OtlobAy7agaOrderDriver> Drivers { get; set; }
        public virtual ICollection<PaymentHistory> PaymentHistories { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
        public virtual ICollection<OtlobAy7agaOrderChat> Chats { get; set; }
        public bool IsRefundRequired { get; set; }
        public int FinishCode { get; set; }
    }
}