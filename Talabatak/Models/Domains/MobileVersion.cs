using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class MobileVersion : BaseModel
    {
        public string Version { get; set; }
        public bool IsUpdateRequired { get; set; }
        public OS OS { get; set; }
    }
}