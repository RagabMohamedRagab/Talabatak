using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Enums;

namespace Talabatak.Models.DTOs
{
    public class PostPushTokenDTO
    {
        public OS OS { get; set; }
        public string PushToken { get; set; }
        public bool IsDriver { get; set; }
    }
}