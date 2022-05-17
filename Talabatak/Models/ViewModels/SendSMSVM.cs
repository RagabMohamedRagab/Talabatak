using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class SendSMSVM
    {
        public bool IsAllUsers { get; set; }
        public List<string> Users { get; set; }
        public string PhoneNumbers { get; set; }
        [Required(ErrorMessage = "محتوى الرساله مطلوب")]
        public string Message { get; set; }
    }
}