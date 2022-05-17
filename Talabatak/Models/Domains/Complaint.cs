using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Complaint : BaseModel
    {
        public Complaint()
        {
            IsViewed = false;
        }
        public string Message { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public bool IsViewed { get; set; }
    }
}