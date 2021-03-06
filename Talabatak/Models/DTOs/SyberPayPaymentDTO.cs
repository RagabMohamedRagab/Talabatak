using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class SyberPayPaymentDTO
    {
        public string applicationId { get; set; } // Unique ID assigned to the merchant application by SyberPay
        public string payeeId { get; set; } // Unique ID for the merchant as provided by Payment Gateway
        public string serviceId { get; set; } // Unique ID for the merchant service provided by SyberPay
        public string customerRef { get; set; } // Transaction reference ID generated by the merchant application
        public decimal amount { get; set; } // Total amount of the transaction
        public string currency { get; set; } // Currency code of the transaction
        public Dictionary<string, string> paymentInfo { get; set; } = new Dictionary<string, string>(); // Payment information as a key/value map
        public string hash { get; set; } // Hash string to ensure integrity of the message
    }
}