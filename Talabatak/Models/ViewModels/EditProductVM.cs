using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.ViewModels
{
    public class EditProductVM
    {
        public long ProductId { get; set; }
        [Required(ErrorMessage = "المتجر مطلوب")]
        public long StoreId { get; set; }
        [Required(ErrorMessage = "الاسم باللغة العربية مطلوب")]
        public string NameAr { get; set; }
        [Required(ErrorMessage = "الاسم باللغة الانجليزية مطلوب")]
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        [Required(ErrorMessage ="العملة مطلوب")]
        public string Currency { get; set; }
        [Required(ErrorMessage ="العملة بالأنجليزي مطلوب")]
        public string CurrencyAr { get; set; }
        public string DescriptionEn { get; set; }
        public double Inventory { get; set; }
        public decimal? SingleOriginalPrice { get; set; }
        public decimal? SingleOfferPrice { get; set; }
        [Required(ErrorMessage = "فئه المنتج مطلوبة")]
        public long CategoryId { get; set; }
        public List<string> SizesAr { get; set; }
        public List<string> SizesEn { get; set; }
        public List<decimal> OriginalPrices { get; set; }
        public List<decimal> OfferPrices { get; set; }
        public List<HttpPostedFileBase> Images { get; set; }

        public static EditProductVM ToEditProductVM(Product product)
        {
            return new EditProductVM()
            {
                ProductId = product.Id,
                StoreId = product.Category.StoreId,
                CategoryId = product.SubCategoryId,
                NameAr = product.NameAr,
                DescriptionEn = product.DescriptionEn,
                NameEn = product.NameEn,
                SingleOfferPrice = product.SingleOfferPrice,
                SingleOriginalPrice = product.SingleOriginalPrice,
                DescriptionAr = product.DescriptionAr,
                Inventory = product.Inventory,
                Currency=product.Currency,
                CurrencyAr=product.CurrencyAr
            };
            throw new NotImplementedException();
        }
    }
}