using Talabatak.Models.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UserAddressDTO
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        [Required]
        public string Address { get; set; }
        public string Floor { get; set; }
        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string AddressInDetails { get; set; }

        public static UserAddressDTO toUserAddressDTO(UserAddress userAddress)
        {
            return new UserAddressDTO()
            {
                Id = (long)userAddress.Id,
                Address = userAddress.Address,
                AddressInDetails = userAddress.AddressInDetails,
                Apartment = userAddress.Apartment,
                BuildingNumber = userAddress.BuildingNumber,
                Floor = userAddress.Floor,
                Latitude = userAddress.Latitude,
                Longitude = userAddress.Longitude,
                Name = userAddress.Name,
                PhoneNumber = userAddress.PhoneNumber,
            };
        }

        public static List<UserAddressDTO> toListOfUserAddressDTO(List<UserAddress> userAddress)
        {
            List<UserAddressDTO> userAddressDTOs = new List<UserAddressDTO>();
            foreach (var address in userAddress)
            {
                userAddressDTOs.Add(new UserAddressDTO()
                {
                    Id = address.Id,
                    Address = address.Address,
                    AddressInDetails = address.AddressInDetails,
                    Apartment = address.Apartment,
                    BuildingNumber = address.BuildingNumber,
                    Floor = address.Floor,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude,
                    Name = address.Name,
                    PhoneNumber = address.PhoneNumber,
                });
            }
            return userAddressDTOs;
        }

        public static UserAddress toUserAddress(UserAddressDTO userAddressDTO, string CurrentUserId)
        {
            return new UserAddress()
            {
                Address = userAddressDTO.Address,
                AddressInDetails = userAddressDTO.AddressInDetails,
                Apartment = userAddressDTO.Apartment,
                BuildingNumber = userAddressDTO.BuildingNumber,
                Floor = userAddressDTO.Floor,
                Latitude = userAddressDTO.Latitude,
                Longitude = userAddressDTO.Longitude,
                Name = userAddressDTO.Name,
                PhoneNumber = userAddressDTO.PhoneNumber,
                UserId = CurrentUserId
            };
        }
    }
}