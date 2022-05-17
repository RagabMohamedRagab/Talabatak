using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class SecondSlider:BaseModel
    {
        public SecondSlider()
        {
        }
        public int SortingNumber { get; set; }
        public long StoreId { get; set; }
        public virtual Store Store { get; set; }
        public string ImagePath { get; set; }
    }
}