using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Talabatak.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
namespace Talabatak.Controllers.API
{
    [System.Web.Http.Authorize]
    [AllowAnonymous]
    [RoutePrefix("api/Notification")]
    public class NotificationController : ApiController
    {
        // GET: Notification
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        public NotificationController()
        {
            baseResponse = new BaseResponseDTO();
        }
        [HttpGet]
        [Route("UnSeenCount")]
        public IHttpActionResult UnSeenCount(bool IsWorker = false)
        {
            string CurrentUserId = User.Identity.GetUserId();
            if (db.Notifications.Any(x => x.IsSeen == false && x.IsWorker == IsWorker && x.IsDeleted==false&& x.UserId == CurrentUserId))
            {
                baseResponse.Data = db.Notifications.Where(d => d.IsDeleted == false && d.IsWorker == IsWorker && d.IsSeen==false && d.UserId == CurrentUserId).ToList().Count ;
            }
            else
            {
                baseResponse.Data = 0;
            }
            return Ok(baseResponse);
        }
    }
}