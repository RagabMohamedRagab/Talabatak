using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Enums;

namespace Talabatak.Models.Domains
{
    public class StoreOrderChat : BaseModel
    {
        public string FromUserId { get; set; }
        public virtual ApplicationUser FromUser { get; set; }
        public string ToUserId { get; set; }
        public virtual ApplicationUser ToUser { get; set; }
        public string Message { get; set; }
        public long OrderId { get; set; }
        public virtual StoreOrder Order { get; set; }
        public string AttachmentName { get; set; }
        public MediaType? AttachmentFileType { get; set; }
        public string AttachmentUrl { get; set; }
    }
}