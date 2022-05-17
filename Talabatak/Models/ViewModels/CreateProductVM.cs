using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CreateProductVM
    {
        [Required(ErrorMessage = "المتجر مطلوب")]
        public long StoreId { get; set; }
        [Required(ErrorMessage = "الاسم باللغة العربية مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغة الانجليزية مطلوب")]
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        [Required(ErrorMessage = "العمله باللغه الانجليزية مطلوب")]
        public string Currency { get; set; }
        [Required(ErrorMessage = "العمله باللغه مطلوب")]
        public string CurrencyAr { get; set; }
        public decimal? SingleOriginalPrice { get; set; }
        public decimal? SingleOfferPrice { get; set; }
        [Required(ErrorMessage = "فئه المنتج مطلوبة")]
        public long CategoryId { get; set; }
        public double Inventory { get; set; }
        public List<string> SizesAr { get; set; }
        public List<string> SizesEn { get; set; }
        public List<decimal> OriginalPrices { get; set; }
        public List<decimal> OfferPrices { get; set; }
        public List<HttpPostedFileBase> Images { get; set; }
    }
}