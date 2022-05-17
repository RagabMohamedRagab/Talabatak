using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.ViewModels
{
    public class DriverReportVM
    {
        public Driver Driver { get; set; }
        public Worker Worker { get; set; }
        public List<StoreOrder> StoreOrders { get; set; }
        public List<OtlobAy7agaOrder> OtlobAy7AgaOrders { get; set; }
        public List<JobOrder> JobOrders { get; set; }
        public List<DriverReview> driverReviews { get; set; }
        public decimal ForSystem { get; set; }
        public int Done { get; set; }
        public int Cancel { get; set; }
        public int Reject { get; set; }
        public decimal Paid { get; set; }
        public decimal Stil { get; set; }
        public decimal Total { get; set; }

    }
}