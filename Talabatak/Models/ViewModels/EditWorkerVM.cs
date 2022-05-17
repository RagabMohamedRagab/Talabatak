using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class EditWorkerVM
    {
        public long WorkerId { get; set; }
        [Required(ErrorMessage = "الاسم الاول للعامل مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الالكترونى مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الالكترونى غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "الدوله مطلوبة")]
        public long CityId { get; set; }
        [Required(ErrorMessage ="ادخل نسبة الربح")]
        public double Profit { get; set; }

        public HttpPostedFileBase PersonalPhoto { get; set; }
        public HttpPostedFileBase IdentityPhoto { get; set; }
        public string DescriptionAr { get;  set; }
        public string DescriptionEn { get;  set; }

        public static EditWorkerVM ToEditWorkerVM(Worker worker)
        {
            var EditVM = new EditWorkerVM()
            {
                WorkerId = worker.Id,
                Email = worker.User.Email,
                Name = worker.User.Name,
                PhoneNumber = worker.User.PhoneNumber,
                DescriptionAr = worker.DescriptionAr,
                DescriptionEn = worker.DescriptionEn,
                Profit = worker.Profit
            };

            if (worker.User.CityId.HasValue == true)
            {
                EditVM.CityId = worker.User.CityId.Value;
            }
            return EditVM;
        }
    }
}