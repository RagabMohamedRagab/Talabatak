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
    public class SubCategoriesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.SubCategories = db.SubCategories.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.SubCategories = db.SubCategories.Where(x => !x.IsDeleted).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(SubCategory subcategory, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                var LatestSortingNumber = db.SubCategories.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                subcategory.SortingNumber = LatestSortingNumber + 1;
                if (Image != null)
                {
                    subcategory.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.SubCategories.Add(subcategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubCategories = db.SubCategories.Where(x => !x.IsDeleted).ToList();
            return View(subcategory);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            SubCategory subcategory = db.SubCategories.Find(id);
            if (subcategory == null)
                return RedirectToAction("Index");
            return View(subcategory);
        }

        [HttpPost]
        public ActionResult Edit(SubCategory subcategory, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    if (subcategory.ImageUrl != null)
                    {
                        MediaControl.Delete(FilePath.Category, subcategory.ImageUrl);
                    }
                    subcategory.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.Entry(subcategory).State = EntityState.Modified;
                CRUD<SubCategory>.Update(subcategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subcategory);
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var subCategory = db.SubCategories.Find(CatId);
            if (subCategory != null)
            {
                if (subCategory.IsDeleted == false)
                {
                    CRUD<SubCategory>.Delete(subCategory);
                }
                else
                {
                    CRUD<SubCategory>.Restore(subCategory);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long CatId, int Number)
        {
            var subCategory = db.SubCategories.Find(CatId);
            if (subCategory == null)
            {
                return Json(new { Sucess = false, Message = "القسم المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingCategoryNumber = db.SubCategories.FirstOrDefault(w => w.Id != CatId && w.SortingNumber == Number);
            if (ExistingCategoryNumber != null)
            {
                return Json(new { Sucess = false, Message = $"القسم {ExistingCategoryNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            subCategory.SortingNumber = Number;
            CRUD<SubCategory>.Update(subCategory);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
