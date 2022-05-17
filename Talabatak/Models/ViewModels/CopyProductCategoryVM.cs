using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CopyProductCategoryVM
    {
        public long FromStoreId { get; set; }
        public long ToStoreId { get; set; }
        public bool IsAllCategory { get; set; }
        public bool IsCopyProducts { get; set; }
        public long[] Categories { get; set; }
    }
}