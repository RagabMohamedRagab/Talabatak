using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class StoreUser : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long StoreId { get; set; }
        public virtual Store Store { get; set; }
    }
}