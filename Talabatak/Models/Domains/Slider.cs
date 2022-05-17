using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Slider : BaseModel
    {
        public Slider()
        {
        }
        public int SortingNumber { get; set; }
        public long StoreId { get; set; }
        public virtual Store Store { get; set; }
        public string ImagePath { get; set; }
    }
}