using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class CreateStoreUserVM
    {
        public long StoreId { get; set; }
        public string UserId { get; set; }
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        public string Email { get; set; }
        public HttpPostedFileBase Image { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "المدينة مطلوبة")]
        public long CityId { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمه السر يجب ان لا تقل عن 6 أحرف")]
        [Required(ErrorMessage = "كلمه السر مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "تأكيد كلمه السر مطلوبة")]
        [Compare("Password", ErrorMessage = "كلمة السر غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}