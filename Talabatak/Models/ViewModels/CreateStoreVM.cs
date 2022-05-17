using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CreateStoreVM
    {
        [Required(ErrorMessage = "الاسم باللغه العربيه مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغه الانجليزية مطلوب")]
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string AddressAr { get; set; }
        public string AddressEn { get; set; }
        [Required(ErrorMessage = "شعار المتجر مطلوب")]
        public HttpPostedFileBase StoreLogo { get; set; }
        [Required(ErrorMessage = "صورة السجل التجاري مطلوبة")]
        public HttpPostedFileBase TaxReportImage { get; set; }
        public HttpPostedFileBase TaxImage { get; set; }
        public HttpPostedFileBase ValueAddedTaxImage { get; set; }
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
        [Required(ErrorMessage = "اسم المسؤول مطلوب")]
        public string OwnerName { get; set; }
        [Required(ErrorMessage = "رقم هاتف المسؤول مطلوب")]
        public string OwnerPhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "البريد غير صحيح")]
        [Required(ErrorMessage = "بريد المسؤول مطلوب")]
        public string OwnerEmail { get; set; }
        [Required(ErrorMessage ="نسبة الخصم مطلوب")]
        public decimal Profit { get; set; }
        public bool DeliveryBySystem { get; set; }
        public decimal StoreOrdersDeliveryOpenFarePrice { get; set; }
        public double StoreOrdersDeliveryOpenFareKilometers { get; set; }
        public decimal StoreOrdersDeliveryEveryKilometerPrice { get; set; }
        [EmailAddress(ErrorMessage = "البريد الرسمي غير صحيح")]
        [Required(ErrorMessage = "بريد الرسمي مطلوب")]
        public string OfficialEmail { get; set; }
        [Compare("OfficialEmail",ErrorMessage ="البريد الرسمي غير متطابق")]
        [Required(ErrorMessage = "تأكيد البريد الرسمي مطلوب")]
        public string ConfirmOfficialEmail { get; set; }

        [Required(ErrorMessage = "رقم السجل التجاري مطلوب")]
        public string TaxReportNumber { get; set; }
        public bool ValueAddedTax { get; set; }

    }
}