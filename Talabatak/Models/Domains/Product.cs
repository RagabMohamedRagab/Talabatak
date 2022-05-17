using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Product : BaseModel
    {
        public Product()
        {
            SellCounter = 0;
        }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public bool IsMultipleSize { get; set; }
        public decimal? SingleOriginalPrice { get; set; }
        public decimal? SingleOfferPrice { get; set; }
        [ForeignKey(nameof(Category))]
        public long SubCategoryId { get; set; }
        public virtual ProductCategory Category { get; set; }
        [ForeignKey(nameof(Store))]
        public long? StoreId { get; set; }
        public virtual Store Store { get; set; }
        public double Rate { get; set; }
        public string Currency { get; set; }
        public string CurrencyAr { get; set; }
        public double Inventory { get; set; }
        public int SellCounter { get; set; }
        public int SortingNumber { get; set; }
       
        public virtual ICollection<ProductSize> Sizes { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<StoreOrderItem> StoreOrderItems { get; set; }
       
    }
}