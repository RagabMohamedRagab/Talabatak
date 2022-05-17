using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class Notification : BaseModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsSeen { get; set; }
        public bool IsWorker { get; set; }
        public long RequestId { get; set; }
        public NotificationType NotificationType { get; set; }
        public string NotificationLink { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public long? StoreId { get; set; }
        public virtual Store Store { get; set; }
    }
}