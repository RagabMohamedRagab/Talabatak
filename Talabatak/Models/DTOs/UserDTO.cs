using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Image { get; set; }
        public string Token { get; set; }
        public decimal Wallet { get; set; }
        public bool IsExternal { get; set; }

        public static UserDTO ToUserDTO(ApplicationUser user)
        {
            return new UserDTO()
            {
                Email = user.Email,
                Image = string.IsNullOrEmpty(user.ImageUrl) ? null : MediaControl.GetPath(FilePath.Users) + user.ImageUrl,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                Name = user.Name,
                Phone = user.PhoneNumber,
                Wallet = user.Wallet,
                IsExternal = user.RegisterationType == Enums.RegisterationType.External ? true : false,
            };
        }
    }
}