using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CreateNotificationVM
    {
        [Required(ErrorMessage = "عنوان الاشعار مطلوب")]
        public string Title { get; set; }
        [Required(ErrorMessage = "محتوى الاشعار مطلوب")]
        public string Body { get; set; }
        public bool IsDriver { get; set; }
        public bool IsWorker { get; set; }
        public List<string> Users { get; set; }
        public OS OS { get; set; }
    }
}