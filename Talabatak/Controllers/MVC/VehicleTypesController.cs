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
    public class VehicleTypesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.VehicleTypes = db.VehicleTypes.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.VehicleTypes = db.VehicleTypes.Where(x => !x.IsDeleted).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(VehicleType vehicleType)
        {
            if (ModelState.IsValid)
            {
                db.VehicleTypes.Add(vehicleType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VehicleTypes = db.VehicleTypes.Where(x => !x.IsDeleted).ToList();
            return View(vehicleType);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            VehicleType vehicleType = db.VehicleTypes.Find(id);
            if (vehicleType == null)
                return RedirectToAction("Index");
            return View(vehicleType);
        }

        [HttpPost]
        public ActionResult Edit(VehicleType vehicleType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vehicleType).State = EntityState.Modified;
                CRUD<VehicleType>.Update(vehicleType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicleType);
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var VehicleType = db.VehicleTypes.Find(CatId);
            if (VehicleType != null)
            {
                if (VehicleType.IsDeleted == false)
                {
                    CRUD<VehicleType>.Delete(VehicleType);
                }
                else
                {
                    CRUD<VehicleType>.Restore(VehicleType);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
