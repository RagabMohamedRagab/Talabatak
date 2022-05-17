﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class AcceptPaymentInfoDto
    {
        public string auth_token { get; set; }
        public string delivery_needed { get; set; }
        public string amount_cents { get; set; }
        public string currency { get; set; }
        public string merchant_order_id { get; set; }
    }
}