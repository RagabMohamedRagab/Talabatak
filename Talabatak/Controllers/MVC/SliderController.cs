using Talabatak.Models.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.ViewModels;
using System;
using Talabatak.Models.Enums;
using System.Data.Entity;


namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class SliderController : BaseController
    {
        public ActionResult Index()
        {
            List<Slider> Sliders = db.Slider.Include(x=>x.Store).Where(b=>!b.IsDeleted).ToList();
            ViewBag.Sliders = Sliders;
            ViewBag.Stores = new SelectList(db.Stores.ToList(), "Id", "NameAr");
            SliderVM slider = new SliderVM();
            return View(slider);
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long StoreId, int Number)
        {
            var Slider = db.Slider.Find(StoreId);
            if (Slider == null)
            {
                return Json(new { Sucess = false, Message = "المنتج المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }
            Slider.SortingNumber = Number;
            CRUD<Slider>.Update(Slider);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadImage(SliderVM model)
        {
            if (model != null)
            {
                try
                {
                    db.Slider.Add(new Slider
                    {
                        ImagePath = MediaControl.Upload(FilePath.Slider, model.Image),
                        SortingNumber = model.SortedNumber,
                        StoreId=model.StoreId
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
            return RedirectToAction("Index");
        }

        public JsonResult DeleteImage(long ImageId)
        {
            var Image = db.Slider.Find(ImageId);
            if (Image != null)
            {
                Image.IsDeleted = true;
                db.SaveChanges();
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Edit(int id)
        {
            SliderVM Slider = new SliderVM();
            ViewBag.EditSlider = db.Slider.FirstOrDefault(s => s.Id == id);
            return View(Slider);
        }

        [HttpPost]
        public ActionResult Edit(SliderVM model)
        {
            try
            {
                var slider = db.Slider.Find(model.Id);
                if (model.Image != null)
                {
                    slider.ImagePath = MediaControl.Upload(FilePath.Slider, model.Image);
                    slider.IsModified = true;
                }
                CRUD<Slider>.Update(slider);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
            }
            return RedirectToAction("Edit", new { SliderId = model.Id });
        }
    }
}