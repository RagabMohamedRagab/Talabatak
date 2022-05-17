using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/categories")]
    public class CategoriesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public CategoriesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("GetAll")]
        public IHttpActionResult GetAllCategories(string lang = "en")
        {
            var Categories = db.Categories.Where(s => s.IsDeleted == false).OrderBy(w => w.SortingNumber).ToList();
            List<CategoryDTO> categoryDTOs = new List<CategoryDTO>();
            foreach (var category in Categories)
            {
                categoryDTOs.Add(new CategoryDTO()
                {
                    Id = category.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                    ImageUrl = !string.IsNullOrEmpty(category.ImageUrl) ? MediaControl.GetPath(FilePath.Category) + category.ImageUrl : null,
                   
                });
            }
            baseResponse.Data = categoryDTOs;
            return Ok(baseResponse);
        }

    }
}