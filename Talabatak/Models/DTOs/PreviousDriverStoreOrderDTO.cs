using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class PreviousDriverStoreOrderDTO
    {
        public long OrderId { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public double? StoreLatitude { get; set; }
        public double? StoreLongitude { get; set; }
        public string StoreImageUrl { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal Total { get; set; }
        public string ClientAddress { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public bool IsUserReviewedDriver { get; set; }
        public string DeliveredOn { get; set; }
    }
}