using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class ProductImage : BaseModel
    {
        public string ImageUrl { get; set; }
        public long ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}