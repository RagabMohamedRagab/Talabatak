using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class EditDriverVM
    {
        public long DriverId { get; set; }
        [Required(ErrorMessage = "الاسم الاول للسائق مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "الدوله مطلوبة")]
        public long CityId { get; set; }

        public long? VehicleTypeId { get; set; }
        public string VehicleNumber { get; set; }
        [Required(ErrorMessage = "ادخل نسبة الربح")]
        public double Profit { get; set; }
        public string VehicleColor { get; set; }
        public HttpPostedFileBase PersonalPhoto { get; set; }
        public HttpPostedFileBase IdentityPhoto { get; set; }
        public HttpPostedFileBase LicensePhoto { get; set; }
        public HttpPostedFileBase VehicleLicensePhoto { get; set; }

        public static EditDriverVM ToEditDriverVM(Driver driver)
        {
            var EditVM = new EditDriverVM()
            {
                DriverId = driver.Id,
                Email = driver.User.Email,
                Name = driver.User.Name,
                PhoneNumber = driver.User.PhoneNumber,
                VehicleTypeId = driver.VehicleTypeId,
                VehicleNumber = driver.VehicleNumber,
                VehicleColor = driver.VehicleColor,
                Profit = driver.Profit
            };

            if (driver.User.CityId.HasValue == true)
            {
                EditVM.CityId = driver.User.CityId.Value;
            }
            return EditVM;
        }
    }
}