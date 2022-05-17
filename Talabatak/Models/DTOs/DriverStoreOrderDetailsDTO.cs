using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class DriverStoreOrderDetailsDTO
    {
        public long OrderId { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        public string DeliverDate { get; set; }
        public string DeliverTime { get; set; }
        public string Code { get; set; }
        public string StoreImageUrl { get; set; }
        public string StoreName { get; set; }
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public decimal DeliveryFees { get; set; }
        public string OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodText { get; set; }
        public UserAddressDTO UserAddress { get; set; }
        public bool IsUserReviewedDriver { get; set; }
        public string UserDriverReview { get; set; }
        public int UserDriverRate { get; set; }
        public string UserImage { get; set; }
        public bool CanCallCustomer { get; set; }
        public List<BasketItemDTO> Items { get; set; } = new List<BasketItemDTO>();
    }
}