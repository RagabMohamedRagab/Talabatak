using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Worker : BaseModel
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string IdentityPhoto { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public bool IsBlocked { get; set; }
        public bool? IsAccepted { get; set; } //Accepted to work in Talabatak
        public double Rate { get; set; }
        public double Profit { get; set; }
        public int NumberOfOrders { get; set; }
        public virtual ICollection<JobOrder> JobOrders { get; set; }
        public virtual ICollection<JobWorker> Jobs { get; set; }
    }
}