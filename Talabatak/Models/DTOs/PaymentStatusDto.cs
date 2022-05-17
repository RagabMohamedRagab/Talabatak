using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class PaymentStatusDto
    {
        public string transactionId { get; set; }
        public string hash { get; set; }
        public string applicationId { get; set; }
        public string token { get; set; }
        public string customerRef { get; set; }
    }
}