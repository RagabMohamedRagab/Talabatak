using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.DTOs
{
    public class UserReportDto
    {
        public ApplicationUser User { get; set; }
        public List<StoreOrder> StoreOrders { get; set; }
        public List<OtlobAy7agaOrder> OtlobAy7AgaOrders { get; set; }
        public List<JobOrder> JobOrders { get; set; }
    }
}