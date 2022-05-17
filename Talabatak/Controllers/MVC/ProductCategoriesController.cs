using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.Models.ViewModels;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin, Store")]
    public class ProductCategoriesController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.ProductCategories = db.ProductCategories.Include(b => b.Category).Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.ProductCategories = db.ProductCategories.Include(b => b.Category).Where(x => !x.IsDeleted).ToList();
            }
            ViewBag.Categories = db.Categories.Include(b => b.ProductCategories).Where(x => !x.IsDeleted).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Index(ProductCategory category, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            category.StoreId = 1;

            if (ModelState.IsValid)
            {
                var LatestSortingNumber = db.ProductCategories.Include(b => b.Store).Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                category.SortingNumber = LatestSortingNumber + 1;
                if (Image != null)
                {
                    category.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.ProductCategories.Add(category);
                db.SaveChanges();
                ViewBag.Categories = db.ProductCategories.Where(x => !x.IsDeleted).ToList();
                ViewBag.ProductCategories = db.ProductCategories.Include(b => b.Category).Where(x => !x.IsDeleted).ToList();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }


        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            ProductCategory category = db.ProductCategories.Find(id);
            if (category == null)
                return RedirectToAction("Index");
            return View(category);
        }

        [HttpPost]
        public ActionResult Edit(ProductCategory category, HttpPostedFileBase Image)
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
                    if (category.ImageUrl != null)
                    {
                        MediaControl.Delete(FilePath.Category, category.ImageUrl);
                    }
                    category.ImageUrl = MediaControl.Upload(FilePath.Category, Image);
                }
                db.Entry(category).State = EntityState.Modified;
                CRUD<ProductCategory>.Update(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
        public ActionResult GetCategories(long StoreId)
        {
            var ProductCategories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == StoreId).ToList();
            return Json(new { Success = true, Data = ProductCategories.Select(s => new { s.Id, s.NameAr }) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(long? ProductId)
        {
            if (ProductId.HasValue == true)
            {
                var Product = db.Products.Find(ProductId.Value);
                if (Product != null)
                {
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        return View(Product);
                    }
                    else
                    {
                        if (Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId))
                        {
                            return View(Product);
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? CatId)
        {
            if (CatId.HasValue == false)
                return RedirectToAction("Index");

            var Category = db.ProductCategories.Find(CatId);
            if (Category != null)
            {
                if (Category.IsDeleted == false)
                {
                    CRUD<ProductCategory>.Delete(Category);
                }
                else
                {
                    CRUD<ProductCategory>.Restore(Category);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }





        [HttpGet]
        public ActionResult CopyCategory()
        {
            ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult CopyCategory(CopyProductCategoryVM model)
        {
            var FromStore = db.Stores.FirstOrDefault(w => w.IsDeleted == false && w.Id == model.FromStoreId);
            if (FromStore == null)
            {
                ModelState.AddModelError("FromStoreId", "المتجر المطلوب غير متاح");
                goto Jump;
            }

            var ToStore = db.Stores.FirstOrDefault(w => w.IsDeleted == false && w.Id == model.ToStoreId);
            if (ToStore == null)
            {
                ModelState.AddModelError("ToStoreId", "المتجر المطلوب غير متاح");
                goto Jump;
            }

            if (model.FromStoreId == model.ToStoreId)
            {
                ModelState.AddModelError("FromStoreId", "لا يمكن اختيار نفس المتجر للنسخ");
                goto Jump;
            }

            if (FromStore.ProductCategories == null || FromStore.ProductCategories.Count(w => w.IsDeleted == false) <= 0)
            {
                ModelState.AddModelError("FromStoreId", "هذا المتجر لا يحتوى على اى اقسام");
                goto Jump;
            }

            if (model.IsAllCategory == false && (model.Categories == null || model.Categories.Any() == false))
            {
                ModelState.AddModelError("Categories", "برجاء اختيار الاقسام المراد نسخها");
                goto Jump;
            }

            if (model.IsAllCategory == true)
            {
                var Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == model.FromStoreId).ToList();
                foreach (var category in Categories)
                {
                    var prodCategory = new ProductCategory()
                    {
                        NameAr = category.NameAr,
                        NameEn = category.NameEn,
                        StoreId = model.ToStoreId
                    };
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        prodCategory.ImageUrl = MediaControl.CopyFile(category.ImageUrl, FilePath.Product, FilePath.Product, MediaType.Image);
                    }
                    db.ProductCategories.Add(prodCategory);
                    db.SaveChanges();
                    if (model.IsCopyProducts == true && category.Products != null && category.Products.Count(w => w.IsDeleted == false) > 0)
                    {
                        foreach (var product in category.Products.Where(s => s.IsDeleted == false))
                        {
                            var newProduct = new Product()
                            {
                                SubCategoryId = prodCategory.Id,
                                DescriptionAr = product.DescriptionAr,
                                DescriptionEn = product.DescriptionEn,
                                IsMultipleSize = product.IsMultipleSize,
                                NameAr = product.NameAr,
                                NameEn = product.NameEn,
                                Rate = product.Rate,
                                SellCounter = 0,
                                SingleOfferPrice = product.SingleOfferPrice,
                                SingleOriginalPrice = product.SingleOriginalPrice,
                            };
                            db.Products.Add(newProduct);
                            if (product.Images != null)
                            {
                                foreach (var image in product.Images.Where(s => s.IsDeleted == false && !string.IsNullOrEmpty(s.ImageUrl)))
                                {
                                    db.ProductImages.Add(new ProductImage()
                                    {
                                        ImageUrl = MediaControl.CopyFile(image.ImageUrl, FilePath.Product, FilePath.Product, MediaType.Image),
                                        ProductId = newProduct.Id
                                    });
                                }
                            }
                            if (product.Sizes != null)
                            {
                                foreach (var size in product.Sizes.Where(s => s.IsDeleted == false))
                                {
                                    db.ProductSizes.Add(new ProductSize()
                                    {
                                        OfferPrice = size.OfferPrice,
                                        OriginalPrice = size.OriginalPrice,
                                        ProductId = newProduct.Id,
                                        SizeAr = size.SizeAr,
                                        SizeEn = size.SizeEn,
                                    });
                                }
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            else
            {
                if (model.Categories != null && model.Categories.Any() == true)
                {
                    foreach (var CategoryId in model.Categories)
                    {
                        var category = db.ProductCategories.FirstOrDefault(w => w.IsDeleted == false && w.Id == CategoryId);
                        if (category != null)
                        {
                            var prodCategory = new ProductCategory()
                            {
                                NameAr = category.NameAr,
                                NameEn = category.NameEn,
                                StoreId = model.ToStoreId
                            };
                            if (!string.IsNullOrEmpty(category.ImageUrl))
                            {
                                prodCategory.ImageUrl = MediaControl.CopyFile(category.ImageUrl, FilePath.Product, FilePath.Product, MediaType.Image);
                            }
                            db.ProductCategories.Add(prodCategory);
                            db.SaveChanges();
                            if (model.IsCopyProducts == true && category.Products != null && category.Products.Count(w => w.IsDeleted == false) > 0)
                            {
                                foreach (var product in category.Products.Where(s => s.IsDeleted == false))
                                {
                                    var newProduct = new Product()
                                    {
                                        SubCategoryId = prodCategory.Id,
                                        DescriptionAr = product.DescriptionAr,
                                        DescriptionEn = product.DescriptionEn,
                                        IsMultipleSize = product.IsMultipleSize,
                                        NameAr = product.NameAr,
                                        NameEn = product.NameEn,
                                        Rate = product.Rate,
                                        SellCounter = 0,
                                        SingleOfferPrice = product.SingleOfferPrice,
                                        SingleOriginalPrice = product.SingleOriginalPrice,
                                    };
                                    db.Products.Add(newProduct);
                                    if (product.Images != null)
                                    {
                                        foreach (var image in product.Images.Where(s => s.IsDeleted == false))
                                        {
                                            db.ProductImages.Add(new ProductImage()
                                            {
                                                ImageUrl = MediaControl.CopyFile(image.ImageUrl, FilePath.Product, FilePath.Product, MediaType.Image),
                                                ProductId = newProduct.Id
                                            });
                                        }
                                    }
                                    if (product.Sizes != null)
                                    {
                                        foreach (var size in product.Sizes.Where(s => s.IsDeleted == false))
                                        {
                                            db.ProductSizes.Add(new ProductSize()
                                            {
                                                OfferPrice = size.OfferPrice,
                                                OriginalPrice = size.OriginalPrice,
                                                ProductId = newProduct.Id,
                                                SizeAr = size.SizeAr,
                                                SizeEn = size.SizeEn,
                                            });
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            TempData["Success"] = true;
            return RedirectToAction("CopyCategory");
            Jump:
            ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
            return View(model);
        }

        [HttpGet]
        public JsonResult GetProductCategories(long StoreId)
        {
            var Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == StoreId).Select(s => new { s.Id, s.NameAr }).ToList();
            return Json(new { Success = true, Data = Categories }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long CatId, int Number)
        {
            var Category = db.ProductCategories.Find(CatId);
            if (Category == null)
            {
                return Json(new { Sucess = false, Message = "القسم المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingCategoryNumber = db.ProductCategories.FirstOrDefault(w => w.Id != CatId && w.StoreId == Category.StoreId && w.SortingNumber == Number);
            if (ExistingCategoryNumber != null)
            {
                return Json(new { Sucess = false, Message = $"القسم {ExistingCategoryNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            Category.SortingNumber = Number;
            CRUD<ProductCategory>.Update(Category);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}







