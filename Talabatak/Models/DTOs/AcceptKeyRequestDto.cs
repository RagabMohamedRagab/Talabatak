using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class AcceptKeyRequestDto
    {
        public string auth_token { get; set; }
        public string amount_cents { get; set; }
        public int expiration { get; set; }
        public string order_id { get; set; }
        public Dictionary<string, string> billing_data { get; set; } = new Dictionary<string, string>();
        public string currency { get; set; }
        public int integration_id { get; set; }
    }
}