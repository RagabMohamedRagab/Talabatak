using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string Name { get; set; }

        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Phone { get; set; }
        public string Image { get; set; }
        //[Required]
        //public long CityId { get; set; }
    }
}