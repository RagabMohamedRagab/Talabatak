using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class JobOrderImage : BaseModel
    {
        public string ImageUrl { get; set; }
        public long OrderId { get; set; }
        public virtual JobOrder Order { get; set; }
    }
}