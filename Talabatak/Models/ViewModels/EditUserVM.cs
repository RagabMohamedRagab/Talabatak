using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class EditUserVM
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        public string Email { get; set; }
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }
        public long? CityId { get; set; }
        public string ImageUrl { get; set; }
    }
}