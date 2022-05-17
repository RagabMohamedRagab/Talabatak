using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.ViewModels
{
    public class StoreReportVM
    {
        public Store Store { get; set; }

        public int ProductCount { get; set; }

        public int ProductCategoriesCount { get; set; }
        public int OwnersCount { get; set; }
        public List<StoreReview> StoreReviews { get; set; }
        public List<StoreOrder> StoreOrders { get; set; }
        public decimal ForSystem { get; set; }
    }
}