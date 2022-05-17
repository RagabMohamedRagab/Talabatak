using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class CheckOutStoreOrderDTO
    {
        public long? UserAddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}