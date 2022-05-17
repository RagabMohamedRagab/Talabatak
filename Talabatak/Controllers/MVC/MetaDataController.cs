using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class MetaDataController : BaseController
    {
        public ActionResult Index()
        {
            return View(db.MetaDatas.FirstOrDefault(w => w.IsDeleted == false));
        }

        [HttpPost]
        public ActionResult Index(MetaData metaData)
        {
            if (ModelState.IsValid == false)
            {
                return View(metaData);
            }
            else
            {
                var Data = db.MetaDatas.FirstOrDefault(d => d.IsDeleted == false);
                if (Data != null)
                {
                    CRUD<MetaData>.Update(metaData);
                    db.Entry(Data).State = System.Data.Entity.EntityState.Detached;
                    db.Entry(metaData).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    db.MetaDatas.Add(metaData);
                    db.SaveChanges();
                }
            }
            TempData["Success"] = true;
            return RedirectToAction("Index");
        }
    }
}