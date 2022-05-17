using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/company")]
    public class CompanyController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public CompanyController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("about")]
        [HttpGet]
        public IHttpActionResult GetAboutCompany()
        {
            var CompanyInfo = db.MetaDatas.FirstOrDefault(d => !d.IsDeleted);
            if (CompanyInfo != null)
            {
                var Data = new
                {
                    CompanyInfo.Phone,
                    CompanyInfo.Email,
                    CompanyInfo.Address,
                    CompanyInfo.WhatsApp,
                    CompanyInfo.Twitter,
                    CompanyInfo.Instagram,
                    CompanyInfo.Facebook,
                };
                baseResponse.Data = Data;
                return Ok(baseResponse);
            }
            return Ok(baseResponse);
        }
    }
}
