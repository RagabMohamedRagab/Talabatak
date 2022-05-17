using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class AddItemToBasketDTO
    {
        public int Quantity { get; set; }
        public long ProductId { get; set; }
        public long SizeId { get; set; }
    }
}