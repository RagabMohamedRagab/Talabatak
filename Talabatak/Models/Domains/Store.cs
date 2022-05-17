using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Store : BaseModel
    {
        public Store()
        {
            IsHidden = false;
            IsBlocked = false;
        }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string PhoneNumber { get; set; }
        public int SeenCount { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string AddressAr { get; set; }
        public string AddressEn { get; set; }
        public string LogoImageUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsClosingManual { get; set; }
        public bool Is24HourOpen { get; set; }
        public TimeSpan? OpenFrom { get; set; }
        public TimeSpan? OpenTo { get; set; }
        public bool IsOpen { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool DeliveryBySystem { get; set; }
        public string OfficialEmail { get; set; }
        public string TaxReportImageUrl { get; set; }
        public string TaxImageUrl { get; set; }
        public string ValueAddedTaxImageUrl { get; set; }
        public string TaxReportNumber { get; set; }
        public bool ValueAddedTax { get; set; }
        public decimal StoreOrdersDeliveryOpenFarePrice { get; set; }
        public double StoreOrdersDeliveryOpenFareKilometers { get; set; }
        public decimal StoreOrdersDeliveryEveryKilometerPrice { get; set; }
        public long CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public long CityId { get; set; }
        public virtual City City { get; set; }
        public bool? IsAccepted { get; set; }
        public bool IsHidden { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public DateTime? RejectedOn { get; set; }
        public double Profit { get; set; }
        public string RejectionReason { get; set; }
        public double Rate { get; set; }
        public int SortingNumber { get; set; }
        public decimal Wallet { get; set; }
        public virtual ICollection<StoreUser> Owners { get; set; }
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Slider> Sliders { get; set; }
        public virtual ICollection<SecondSlider> SecondSliders { get; set; }
        public virtual ICollection<StoreReview> Reviews { get; set; }
        public string ResponsiblePersonEmail { get; internal set; }
        public string ResponsiblePersonPhoneNumber { get; internal set; }
        public string ResponsiblePersonName { get; internal set; }
    }
}