using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class JobWorker : BaseModel
    {
        public long JobId { get; set; }
        public virtual Job Job { get; set; }
        public long WorkerId { get; set; }
        public virtual Worker Worker { get; set; }
    }
}