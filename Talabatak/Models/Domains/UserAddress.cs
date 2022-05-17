using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class UserAddress : BaseModel
    {
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Address { get; set; }
        public string AddressInDetails { get; set; }
        public string Floor { get; set; }
        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<StoreOrder> StoreOrders { get; set; }
        public virtual ICollection<OtlobAy7agaOrder> OtlobAy7agaOrders { get; set; }
    }
}