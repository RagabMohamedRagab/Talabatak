using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Talabatak.Web.Controllers.API
{
    public class CountriesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public CountriesController()
        {
            baseResponse = new BaseResponseDTO();
        }

        public IHttpActionResult Get()
        {
            var Countries = db.Countries.Where(x => !x.IsDeleted).ToList();
            baseResponse.Data = CountryDTO.toListOfCountryDTOs(Countries);
            return Ok(baseResponse);
        }
    }
}
