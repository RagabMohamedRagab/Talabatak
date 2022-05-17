using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UserStoreOrderDetailsDTO
    {
        public long OrderId { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        public string DeliverDate { get; set; }
        public string DeliverTime { get; set; }
        public string Code { get; set; }
        public long StoreId { get; set; }
        public string StoreImageUrl { get; set; }
        public string StoreName { get; set; }
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public string DeliveryFees { get; set; }
        public bool HasDriver { get; set; }
        public long? DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhoneNumber { get; set; }
        public string DriverImageUrl { get; set; }
        public string DriverRate { get; set; }
        public bool CanCallDriver { get; set; }
        public int getDriverOrdersCount { get; set; }
        public string OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodText { get; set; }
        public UserAddressDTO UserAddress { get; set; }
        public bool CanReviewDriver { get; set; }
        public string UserDriverReview { get; set; }
        public int UserDriverRate { get; set; }
        public bool CanReviewStore { get; set; }
        public string UserStoreReview { get; set; }
        public int UserStoreRate { get; set; }
        public List<BasketItemDTO> Items { get; set; } = new List<BasketItemDTO>();
        public bool CanCancel { get; set; }
    }
}