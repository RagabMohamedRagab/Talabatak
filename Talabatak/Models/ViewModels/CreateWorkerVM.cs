using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CreateWorkerVM
    {
        public string UserId { get; set; }
        [Required(ErrorMessage = "اسم العامل مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public long CityId { get; set; }

        [Required(ErrorMessage = "كلمه السر مطلوبه")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمه السر يجب ان لا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمه السر مطلوبه")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة السر غير متطابقة")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "الوظائف مطلوبة")]
        public long[] JobIds { get; set; }
        public HttpPostedFileBase PersonalPhoto { get; set; }
        public double Profit { get; set; }
        public HttpPostedFileBase IdentityPhoto { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
    }
}