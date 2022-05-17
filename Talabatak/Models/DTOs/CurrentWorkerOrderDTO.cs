using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class CurrentWorkerOrderDTO
    {
        public OrderType OrderType { get; set; }
        public long OrderId { get; set; }
        public bool IsPending { get; set; }
        public long? ApplicationId { get; set; }
        public bool IsStoreOrder { get; set; }
        public string StoreName { get; set; }
        public string StoreImageUrl { get; set; }
        public double? StoreLatitude { get; set; }
        public double? StoreLongitude { get; set; }
        public string OtlobAy7agaImageUrl { get; set; }
        public string JobOrderImageUrl { get; set; }
        public double? ClientLatitude { get; set; }
        public double? ClientLongitude { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Address { get; set; }
        public string OrderCode { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}