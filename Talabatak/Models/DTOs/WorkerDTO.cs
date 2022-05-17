using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Helpers;
using Talabatak.Models.Domains;

namespace Talabatak.Models.DTOs
{
    public class WorkerDTO
    {
        public long WorkerId { get; set; }
        public long JobWorkerId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Rate { get; set; }
        public string Image { get; set; }
        public bool Worker { get; set; }

        public static DriverDTO toDeliveryManDTO(Worker worker)
        {
            return new DriverDTO()
            {
                FullName = worker.User.Name,
                Id = worker.Id,
                Image = string.IsNullOrEmpty(worker.User.ImageUrl) ? null : MediaControl.GetPath(FilePath.Users) + worker.User.ImageUrl,
                PersonalPhoto = string.IsNullOrEmpty(worker.User.ImageUrl) ? null : MediaControl.GetPath(FilePath.Users) + worker.User.ImageUrl,
                Phone = worker.User.PhoneNumber,
                Latitude = worker.User.Latitude,
                Longitude = worker.User.Longitude,
                IdentityId = worker.UserId,
                AreaAr = worker.User.CityId.HasValue == true ? worker.User.City.NameAr : null,
                AreaEn = worker.User.CityId.HasValue == true ? worker.User.City.NameEn : null,
                AreaId = worker.User.CityId,
                CountryAr = worker.User.CityId.HasValue == true ? worker.User.City.Country.NameAr : null,
                CountryEn = worker.User.CityId.HasValue == true ? worker.User.City.Country.NameEn : null,
                CountryId = worker.User.CityId.HasValue == true ? (long?)worker.User.City.CountryId : null,
                Worker=true
            };
        }
    }
}