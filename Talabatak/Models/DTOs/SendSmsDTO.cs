using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class SendSmsDTO
    {
        public string sender_id { get;  set; }
        public string to { get;  set; }
        public string message { get;  set; }
    }
}