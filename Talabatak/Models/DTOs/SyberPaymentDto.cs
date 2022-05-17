using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class SyberPaymentDto
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string customerRef { get; set; }
    }
}