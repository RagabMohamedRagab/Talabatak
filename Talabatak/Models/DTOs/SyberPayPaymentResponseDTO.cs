using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class SyberPayPaymentResponseDTO
    {
        public string applicationId { get; set; } // Unique ID assigned to the merchant application by SyberPay
        public string payeeId { get; set; } // Unique ID for the merchant as provided by Payment Gateway
        public string serviceId { get; set; } // Unique ID for the merchant service provided by SyberPay
        public string customerRef { get; set; } // Transaction reference ID generated by the merchant application
        public decimal amount { get; set; } // Total amount of the transaction
        public string currency { get; set; } // Currency code of the transaction
        public Dictionary<string, string> paymentInfo { get; set; } = new Dictionary<string, string>(); // Payment information as a key/value map
        public string tranTimestamp { get; set; } // The timestamp of the transaction
        public int responseCode { get; set; } // Code representing the status of the transaction 
        public string responseMessage { get; set; } // Textual Message describing the status of the transaction
        public string paymentUrl { get; set; } // Payment URL generated by SyberPay for this transaction
    }
}