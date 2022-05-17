using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talabatak.Models.Domains
{
    public class Driver : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long? VehicleTypeId { get; set; }
        public virtual VehicleType VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleColor { get; set; }
        public string PersonalPhoto { get; set; }
        public string IdentityPhoto { get; set; }
        public double Profit { get; set; }
        public string LicensePhoto { get; set; }
        public string VehicleLicensePhoto { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? AcceptedOn { get; set; }
        [ForeignKey(nameof(City))]
        public long? CityId { get; set; }
        public virtual City City { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsOnline { get; set; }
        public bool? IsAccepted { get; set; } //Accepted to work in Talabatak
        public bool IsAvailable { get; set; } //if driver is available to deliver an order
        public double Rate { get; set; }
        public int NumberOfCompletedTrips { get; set; }
        public virtual ICollection<StoreOrder> StoreOrders { get; set; }
        public virtual ICollection<StoreOrderDriver> Orders { get; set; }
        public virtual ICollection<DriverReview> Reviews { get; set; }
        public virtual ICollection<OtlobAy7agaOrder> ActualOtlobAy7agaOrders { get; set; }
        public virtual ICollection<OtlobAy7agaOrderDriver> SuggestedOtlobAy7agaOrders { get; set; }
    }
}