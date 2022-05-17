using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Talabatak.Web.Controllers.API
{
    [Authorize]
    public class ComplaintsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public ComplaintsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        public async Task<IHttpActionResult> Post(ComplaintDTO complaint)
        {
            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = Errors.MessageIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            Complaint newComp = new Complaint()
            {
                Message = complaint.Message,
                UserId = User.Identity.GetUserId()
            };

            db.Complaints.Add(newComp);
            await db.SaveChangesAsync();
            return Ok(baseResponse);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}