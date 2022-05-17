using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class AcceptPaymentResponseDto
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public string delivery_needed { get; set; }
        public string amount_cents { get; set; }
        public string currency { get; set; }
        public string is_payment_locked { get; set; }
    }
}