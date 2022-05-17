using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class NotificationDTO
    {
        public long NotificationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
        public bool IsSeen { get; set; }
        public NotificationType Type { get; set; }
        public long RequestId { get; set; }
    }
}