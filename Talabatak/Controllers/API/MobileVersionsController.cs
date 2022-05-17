using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/mobiles/versions")]
    public class MobileVersionsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public MobileVersionsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpPost]
        [Route("NewVersion")]
        public IHttpActionResult AddNewVersion(MobileVersionDTO model)
        {
            var dbObject = db.MobileVersions.FirstOrDefault(w => w.IsDeleted == false && w.OS == model.OS && w.Version == model.Version);
            if (dbObject != null)
            {
                dbObject.IsUpdateRequired = model.IsUpdateRequired;
            }
            else
            {
                db.MobileVersions.Add(new MobileVersion()
                {
                    OS = model.OS,
                    Version = model.Version,
                    IsUpdateRequired = model.IsUpdateRequired
                });
            }
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Checkforupdates")]
        public IHttpActionResult CheckForUpdates(OS OS, string Version)
        {
            var LatestVersion = db.MobileVersions.Where(s => s.IsDeleted == false && s.OS == OS).OrderByDescending(s => s.CreatedOn).FirstOrDefault();
            if (LatestVersion != null)
            {
                if (LatestVersion.Version == Version)
                {
                    baseResponse.Data = new
                    {
                        LatestVersion = Version,
                        IsUpdated = true,
                        IsUpdateRequired = false
                    };
                }
                else
                {
                    baseResponse.Data = new
                    {
                        LatestVersion = LatestVersion.Version,
                        IsUpdated = false,
                        IsUpdateRequired = LatestVersion.IsUpdateRequired
                    };
                }
            }
            else
            {
                baseResponse.Data = new
                {
                    LatestVersion = "",
                    IsUpdated = true,
                    IsUpdateRequired = false
                };
            }
            return Ok(baseResponse);
        }
    }
}