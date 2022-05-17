using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class JobOrder : BaseModel
    {
        public int FinishCode { get; set; }
        public string Code { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public double WorkerProfit { get; set; }
        public string AddressInDetails { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Building { get; set; }
        public string Floor { get; set; }
        public string Apartment { get; set; }
        public string Description { get; set; }
        public JobOrderStatus Status { get; set; }
        public bool IsUserReviewed { get; set; }
        public int? UserReviewRate { get; set; }
        public string UserReviewDescription { get; set; }
        public DateTime? UserReviewDate { get; set; }
        public decimal TotalPrice { get; set; }
        public long WorkerId { get; set; }
        public virtual Worker Worker { get; set; }
        public long JobId { get; set; }
        public virtual Job Job { get; set; }
        public virtual ICollection<JobOrderImage> Images { get; set; }
        public virtual ICollection<JobOrderChat> ChatMessages { get; set; }
    }
}