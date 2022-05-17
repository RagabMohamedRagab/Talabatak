using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UpdateUserDTO
    {
        [Required]
        public string Name { get; set; }

        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }

        [Required]
        public string Phone { get; set; }
        public string Image { get; set; }
        //public long CityId { get; set; }
    }
}