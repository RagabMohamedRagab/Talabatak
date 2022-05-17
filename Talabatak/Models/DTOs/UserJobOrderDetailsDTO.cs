using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UserJobOrderDetailsDTO
    {
        public string WorkerName { get; set; }
        public string WorkerPhoneNumber { get; set; }
        public string WorkerEmail { get; set; }
        public string WorkerRate { get; set; }
        public string Apartment { get; set; }
        public string Address { get; set; }
        public string AddressInDetails { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public long OrderId { get; set; }
        public string OrderDescription { get; set; }
        public bool CanCancel { get; set; }
        public string OrderDate { get; set; }
        public JobOrderStatus OrderStatus { get; set; }
        public string OrderStatusText { get; set; }
        public decimal OrderPrice { get; set; }
        public List<string> OrderImages { get; set; } = new List<string>();
        public bool IsRated { get; set; }
        public int Rate { get; set; }
        public string RateDescription { get; set; }
        public string ClientEmail { get; set; }
        public string ClientName { get; set; }
        public string ClientPhoneNumber { get; set; }
    }
}