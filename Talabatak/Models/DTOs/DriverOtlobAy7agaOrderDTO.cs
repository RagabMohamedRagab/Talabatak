using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class DriverOtlobAy7agaOrderDTO
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string City { get; set; }
        public string PaymentMethodText { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Distance { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public string OrderStatus { get; set; }
        public UserAddressDTO UserAddress { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string UserImage { get; set; }
        public bool CanCallCustomer { get; set; }
    }
}