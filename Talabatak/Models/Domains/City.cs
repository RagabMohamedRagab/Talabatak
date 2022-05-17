using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class City : BaseModel
    {
        public long CountryId { get; set; }
        public virtual Country Country { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<OtlobAy7agaOrder> OtlobAy7agaOrders { get; set; }
        public virtual ICollection<Driver> Drivers { get; set; }
    }
}