using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class AcceptStatusDto
    {
        public Dictionary<string, string> obj { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> order { get; set; } = new Dictionary<string, string>();
        public int amount_cents { get; set; }
    }
}