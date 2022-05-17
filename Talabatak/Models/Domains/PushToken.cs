using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class PushToken : BaseModel
    {
        public string Token { get; set; }
        public OS OS { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public bool IsWorker { get; set; }
    }
}