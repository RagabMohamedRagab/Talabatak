using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Talabatak.Web.Controllers.API
{
    [Authorize]
    public class UserAddressesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public UserAddressesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var CurrentUserId = User.Identity.GetUserId();
            var Addresses = db.UserAddresses.Where(x => x.UserId == CurrentUserId && !x.IsDeleted).OrderByDescending(x => x.CreatedOn).ToList();
            baseResponse.Data = UserAddressDTO.toListOfUserAddressDTO(Addresses);

            return Ok(baseResponse);
        }

        [HttpPost]
        public IHttpActionResult Post(UserAddressDTO addressDTO)
        {
            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = ValidateNewUserAddressApi(ModelState);
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            if (addressDTO.Id == 0)
            {
                var CurrentUserId = User.Identity.GetUserId();
                UserAddress userAddress = UserAddressDTO.toUserAddress(addressDTO, CurrentUserId);
                db.UserAddresses.Add(userAddress);
            }
            else
            {
                var Address = db.UserAddresses.Find(addressDTO.Id);
                if (Address == null)
                {
                    baseResponse.ErrorCode = Errors.UserAddressNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }

                Address.Address = addressDTO.Address;
                Address.AddressInDetails = addressDTO.AddressInDetails;
                Address.Apartment = addressDTO.Apartment;
                Address.BuildingNumber = addressDTO.BuildingNumber;
                Address.Floor = addressDTO.Floor;
                Address.Latitude = addressDTO.Latitude;
                Address.Longitude = addressDTO.Longitude;
                Address.Name = addressDTO.Name;
                Address.PhoneNumber = addressDTO.PhoneNumber;
            }
            db.SaveChanges();

            return Ok(baseResponse);
        }

        private Errors ValidateNewUserAddressApi(ModelStateDictionary Model)
        {
            var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            if (ModelErrors.Any(x => x.Contains("Name")))
                return Errors.NameIsRequired;

            if (ModelErrors.Any(x => x.Contains("Address")))
                return Errors.UserAddressIsRequired;

            if (ModelErrors.Any(x => x.Contains("Floor")))
                return Errors.FloorIsRequired;

            if (ModelErrors.Any(x => x.Contains("BuildingNumber")))
                return Errors.BuildingNumberIsRequired;

            if (ModelErrors.Any(x => x.Contains("Apartment")))
                return Errors.ApartmentIsRequired;

            if (ModelErrors.Any(x => x.Contains("PhoneNumber")))
                return Errors.PhoneIsRequiredAndInCorrectFormat;

            return Errors.SomethingWentWrong;
        }

        [HttpDelete]
        public IHttpActionResult Delete(long UserAddressId)
        {
            var Address = db.UserAddresses.Find(UserAddressId);
            if (Address == null)
            {
                baseResponse.ErrorCode = Errors.UserAddressNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            CRUD<UserAddress>.Delete(Address);
            db.SaveChanges();
            return Ok(baseResponse);
        }

        
    }
}
