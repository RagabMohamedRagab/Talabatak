using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class EditStoreVM
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "الاسم باللغه العربيه مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغه الانجليزية مطلوب")]
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string AddressAr { get; set; }
        public string AddressEn { get; set; }
        public HttpPostedFileBase StoreLogo { get; set; }
        public HttpPostedFileBase CoverImage { get; set; }
        [Required(ErrorMessage = "توقيت المتجر مطلوب")]
        public bool Is24HourOpen { get; set; }
        [Required(ErrorMessage = "التوقيت من مطلوب")]
        public string From { get; set; }
        [Required(ErrorMessage = "التوقيت الى مطلوب")]
        public string To { get; set; }
        [Required(ErrorMessage = "موقع المتجر على الخريطه مطلوب")]
        public double? Latitude { get; set; }
        [Required(ErrorMessage = "موقع المتجر على الخريطه مطلوب")]
        public double? Longitude { get; set; }
        [Required(ErrorMessage = "قسم المتجر مطلوب")]
        public long CategoryId { get; set; }
        [Required(ErrorMessage = "مدينة المتجر مطلوبة")]
        public long CityId { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }
        public TimeSpan? OldFrom { get; set; }
        public TimeSpan? OldTo { get; set; }
        public decimal Profit { get; set; }
        public bool DeliveryBySystem { get; set; }
        public decimal StoreOrdersDeliveryOpenFarePrice { get; set; }
        public double StoreOrdersDeliveryOpenFareKilometers { get; set; }
        public decimal StoreOrdersDeliveryEveryKilometerPrice { get; set; }
    }
}