using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class SliderVM
    {
        public int Id { get; set; }
        public HttpPostedFileBase Image { get; set; }
        public long StoreId { get; set; }

        public int SortedNumber { get; set; }


    }
}