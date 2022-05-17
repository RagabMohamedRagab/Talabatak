using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class ComplaintsController : BaseController
    {
        public ActionResult Index()
        {
            return View(db.Complaints.Where(s => s.IsDeleted == false).OrderBy(s => s.IsViewed).ThenByDescending(c => c.CreatedOn).ToList());
        }

        [HttpGet]
        public ActionResult ViewMessage(long Id)
        {
            var Message = db.Complaints.Find(Id);
            if (Message == null)
                return Json(new { Success = false, Message = "الرساله غير متاحه" }, JsonRequestBehavior.AllowGet);

            Message.IsViewed = true;
            db.SaveChanges();
            return PartialView("_MessageModal", Message);
        }
    }
}