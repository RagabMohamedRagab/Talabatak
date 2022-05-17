using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class NotificationVM
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsSeen { get; set; }
        public long RequestId { get; set; }
        public NotificationType NotificationType { get; set; }
        public string Link { get; set; }
    }
}