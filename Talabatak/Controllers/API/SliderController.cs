using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;

namespace Talabatak.Controllers.API
{
    [RoutePrefix("api/Slider")]
    public class SliderController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public SliderController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("Get")]
        public async Task<IHttpActionResult> GetSliders()
        {
            try
            {
                var sliders =  await db.Slider
                    .Where(s => s.IsDeleted != true)
                    .Select(s=> new SliderDTO {
                    ID = s.Id , 
                    Image = "/Content/Images/Slider/" + s.ImagePath ,
                    StoreId = s.StoreId,
                    SortingNumber = s.SortingNumber
                    }).ToListAsync();
                var Secoundsliders = await db.SecondSliders
                   .Where(s => s.IsDeleted != true)
                   .Select(s => new SecondSliderDTO
                   {
                       ID = s.Id,
                       Image = "/Content/Images/Slider/" + s.ImagePath,
                       StoreId = s.StoreId,
                       SortingNumber = s.SortingNumber
                   }).ToListAsync();
                var dto = new TwoSliderDTO()
                {
                    SliderDTOs = sliders,
                    SecondSliderDTOs = Secoundsliders
                };
                baseResponse.Data = dto;
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
            return Ok(baseResponse);
        }
    }
}