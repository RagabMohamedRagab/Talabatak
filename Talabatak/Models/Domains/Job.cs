using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Job : BaseModel
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfOrders { get; set; }
        public int SortingNumber { get; set; }
        public virtual ICollection<JobWorker> Workers { get; set; }
        public virtual ICollection<JobOrder> Orders { get; set; }
    }
}