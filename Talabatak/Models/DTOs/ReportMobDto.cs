using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class ReportMobDto
    {
        public string Mobile { get; set; }
        public string UserName { get; set; }
        public decimal Wallet { get; set; }
        public double Rate { get; set; }
        public int StoreOrders { get; set; }
        public int OtlobAy7AgaOrders { get; set; }
        public int JobOrders { get; set; }
        public int driverReviews { get; set; }
        public decimal ForSystem { get; set; }
        public decimal Paid { get; set; }
        public decimal Stil { get; set; }
        public decimal Total { get; set; }
    }
}