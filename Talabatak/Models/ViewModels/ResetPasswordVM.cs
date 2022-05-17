using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class ResetPasswordVM
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "ادخل كلمة السر")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password" , ErrorMessage ="كلمة سر غير متطابقة")]
        public string ConfirmPassword { get; set; }
    }
}