using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class PaymentStatusResDto
    {
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public string customerRef { get; set; }
    }
}