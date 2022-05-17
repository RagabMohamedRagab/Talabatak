using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class OtlobAy7agaOrderDriver : BaseModel
    {
        public long OrderId { get; set; }
        public virtual OtlobAy7agaOrder Order { get; set; }
        public long DriverId { get; set; }
        public virtual Driver Driver { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public DateTime? RejectedOn { get; set; }
    }
}