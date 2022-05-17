using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class PushTokenDTO
    {
        public string PushToken { get; set; }
        public OS OS { get; set; }
    }
}