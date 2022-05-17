using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class SubCategory: BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool Visiable { get; set; }
        public string ImageUrl { get; set; }
        public int SortingNumber { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
    }
}