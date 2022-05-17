using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class ProductCategory : BaseModel
    {
        public long StoreId { get; set; }
        public virtual Store Store { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
        [ForeignKey(nameof(Category))]
        public long? CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public int SortingNumber { get; set; }
        public virtual ICollection<Product> Products { get; set; }

        
    }
}