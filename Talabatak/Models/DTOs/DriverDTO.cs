using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class DriverDTO
    {
        public long Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? CountryId { get; set; }
        public string CountryAr { get; set; }
        public string CountryEn { get; set; }
        public long? AreaId { get; set; }
        public string AreaAr { get; set; }
        public string AreaEn { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleColor { get; set; }
        public string PersonalPhoto { get; set; }
        public string IdentityId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Token { get; set; }
        public string Image { get; set; }
        public bool Worker { get; set; }

        public static DriverDTO toDeliveryManDTO(Driver deliveryMan)
        {
            return new DriverDTO()
            {
                FullName = deliveryMan.User.Name,
                Id = deliveryMan.Id,
                Image = string.IsNullOrEmpty(deliveryMan.User.ImageUrl) ? null : MediaControl.GetPath(FilePath.Users) + deliveryMan.User.ImageUrl,
                PersonalPhoto = string.IsNullOrEmpty(deliveryMan.PersonalPhoto) ? null : MediaControl.GetPath(FilePath.Users) + deliveryMan.PersonalPhoto,
                Phone = deliveryMan.User.PhoneNumber,
                Latitude = deliveryMan.User.Latitude,
                Longitude = deliveryMan.User.Longitude,
                IdentityId = deliveryMan.UserId,
                AreaAr = deliveryMan.User.CityId.HasValue == true ? deliveryMan.User.City.NameAr : null,
                AreaEn = deliveryMan.User.CityId.HasValue == true ? deliveryMan.User.City.NameEn : null,
                AreaId = deliveryMan.User.CityId,
                CountryAr = deliveryMan.User.CityId.HasValue == true ? deliveryMan.User.City.Country.NameAr : null,
                CountryEn = deliveryMan.User.CityId.HasValue == true ? deliveryMan.User.City.Country.NameEn : null,
                CountryId = deliveryMan.User.CityId.HasValue == true ? (long?)deliveryMan.User.City.CountryId : null,
                VehicleColor = deliveryMan.VehicleColor,
                VehicleNumber = deliveryMan.VehicleNumber,
                Worker = false
            };
        }
    }
}