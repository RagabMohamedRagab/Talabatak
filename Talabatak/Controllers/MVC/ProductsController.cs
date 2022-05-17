using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin, Store")]
    public class ProductsController : BaseController
    {
        public ActionResult Index(long? StoreId , long? DeletedProduct)
        {
            if(DeletedProduct.HasValue && DeletedProduct.Value == 1)
            {
                ViewBag.Tit = "Done";
                ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                var Products = db.Products.Include(x => x.Category).Include(x => x.Category.Store)
                   .Where(s => s.IsDeleted || s.Category.IsDeleted || s.Category.Store.IsDeleted).OrderByDescending(s => s.CreatedOn).ToList();
                return View(Products);
            }
            ViewBag.StoreId = StoreId;
            if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
            {
                var Products = db.Products.Include(x=>x.Category).Include(x=>x.Category.Store)
                    .Where(s=> !s.IsDeleted && !s.Category.IsDeleted && !s.Category.Store.IsDeleted).OrderByDescending(s => s.CreatedOn).ToList();
                if (StoreId.HasValue == true)
                {
                    Products = Products.Where(w => w.Category.StoreId == StoreId.Value).ToList();
                }
                ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                return View(Products);
            }
            else if (User.IsInRole("Store"))
            {
                if (StoreId.HasValue == true)
                {
                    var Store = db.StoreUsers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.StoreId == StoreId.Value);
                    if (Store != null)
                    {
                        ViewBag.Store = Store.Store;
                        ViewBag.Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(q => q.Store).ToList();
                        var Products = db.Products.Include(x => x.Category).Include(x => x.Category.Store)
                            .Where(w => w.Category.StoreId == StoreId.Value &&
                        ! w.IsDeleted && !w.Category.IsDeleted && !w.Category.Store.IsDeleted)
                            .ToList();
                        return View(Products);
                    }
                }
                else
                {
                    ViewBag.Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(q => q.Store).ToList();
                    return View();
                }
            }
            return RedirectToAction("Index", "Cp");
        }
        [HttpGet]
        public ActionResult Create()
        {
            List<Store> Stores = null;
            if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
            {
                Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
            }
            else if (User.IsInRole("Store"))
            {
                Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(x => x.Store).ToList();
                if (Stores == null || Stores.Count() <= 0)
                {
                    return RedirectToAction("Index", "Cp");
                }
            }
            if (Stores == null || Stores.Count() <=0)
            { return RedirectToAction("Index", "Cp"); }
            else
            {
                ViewBag.Stores = Stores;
                long FirstStoreId = Stores.FirstOrDefault().Id;
                ViewBag.Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == FirstStoreId).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateProductVM model)
        {
            model.StoreId = 1;
            try
            {
                var Errors = ProductCreationValidation(model);
                if (Errors.Count() > 0)
                {
                    goto Return;
                }
                else
                {
                    using (var Transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var LatestSortingNumber = db.Products.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                            Product product = new Product()
                            {
                                SubCategoryId = model.CategoryId,
                                DescriptionAr = model.DescriptionAr,
                                DescriptionEn = model.DescriptionEn,
                                NameAr = model.NameAr,
                                NameEn = model.NameEn,
                                Currency=model.Currency,
                                CurrencyAr=model.CurrencyAr,
                                SortingNumber = LatestSortingNumber + 1,
                                Inventory=model.Inventory,
                                 SingleOfferPrice=model.SingleOfferPrice,
                                  SingleOriginalPrice=model.SingleOriginalPrice,
                                  StoreId=1,
                            };
                            if (model.OriginalPrices != null && model.OriginalPrices.Count() > 0)
                            {
                                product.IsMultipleSize = true;
                            }
                            else
                            {
                                product.SingleOriginalPrice = model.SingleOriginalPrice;
                                product.SingleOfferPrice = model.SingleOfferPrice;
                                product.IsMultipleSize = false;
                            }
                            db.Products.Add(product);
                            db.SaveChanges();
                            if (model.OriginalPrices != null && model.OriginalPrices.Count() > 0)
                            {
                                for (int i = 0; i < model.OriginalPrices.Count(); i++)
                                {
                                    if (string.IsNullOrEmpty(model.SizesAr.ElementAtOrDefault(i)) || string.IsNullOrEmpty(model.SizesEn.ElementAtOrDefault(i)))
                                    {
                                        continue;
                                    }
                                    db.ProductSizes.Add(new ProductSize()
                                    {
                                        OfferPrice = model.OfferPrices.ElementAtOrDefault(i) > 0 ? (decimal?)model.OfferPrices.ElementAtOrDefault(i) : null,
                                        OriginalPrice = model.OriginalPrices.ElementAtOrDefault(i),
                                        SizeAr = model.SizesAr.ElementAtOrDefault(i),
                                        SizeEn = model.SizesEn.ElementAtOrDefault(i),
                                        ProductId = product.Id
                                    });
                                }
                            }
                            db.SaveChanges();
                            if (model.Images != null)
                            {
                                foreach (var image in model.Images.Where(w => w != null))
                                {
                                    db.ProductImages.Add(new ProductImage()
                                    {
                                        ImageUrl = MediaControl.Upload(FilePath.Product, image),
                                        ProductId = product.Id
                                    });
                                }
                            }
                            db.SaveChanges();
                            Transaction.Commit();
                            return Json(new { Success = true, StoreId = model.StoreId }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            Errors.Add("برجاء المحاولة فى وقت لاحق");
                            goto Return;
                        }
                    }
                }
                Return:
                return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return Json(new { Success = false, Errors = new List<string>() { "برجاء المحاولة فى وقت لاحق" } }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<string> ProductCreationValidation(CreateProductVM model)
        {
            List<string> Errors = new List<string>();
            var Store = db.Stores.FirstOrDefault(s => !s.IsDeleted  && s.Id == model.StoreId);
            if (Store == null)
                Errors.Add("المتجر المطلوب غير متاح");

            var Category = db.ProductCategories.FirstOrDefault(s => s.IsDeleted == false && s.Id == model.CategoryId && s.StoreId == model.StoreId);
            if (Category == null)
                Errors.Add("القسم او الفئة المطلوبة غير متاحة");

            if (string.IsNullOrEmpty(model.NameAr) || string.IsNullOrEmpty(model.NameEn))
                Errors.Add("الاسم مطلوب");

            if (model.Images != null)
            {
                foreach (var image in model.Images.Where(w => w != null))
                {
                    if (CheckFiles.IsImage(image) == false)
                    {
                        Errors.Add("احدى الصور غير صحيحة");
                        break;
                    }
                }
            }

            if (model.SingleOriginalPrice.HasValue == false)
            {
                if (model.OriginalPrices == null || model.OriginalPrices.Count() <= 0)
                {
                    Errors.Add("برجاء كتابة السعر او اختيار احجام");
                }
            }
            else
            {
                if (model.SingleOfferPrice.HasValue == true && model.SingleOfferPrice.Value > model.SingleOriginalPrice.Value)
                    Errors.Add("السعر بعد الخصم غير صحيح");
            }

            if (model.SizesAr != null || model.SizesEn != null || model.OriginalPrices != null || model.OfferPrices != null)
            {
                int SizesArLength = -1;
                if (model.SizesAr != null)
                    SizesArLength = model.SizesAr.Count();

                int SizesEnLength = -1;
                if (model.SizesEn != null)
                    SizesEnLength = model.SizesEn.Count();

                int OriginalPricesLength = -1;
                if (model.OriginalPrices != null)
                    OriginalPricesLength = model.OriginalPrices.Count();

                if (SizesArLength != SizesEnLength || SizesArLength != OriginalPricesLength || SizesEnLength != OriginalPricesLength || SizesEnLength < 0 || SizesArLength < 0 || OriginalPricesLength < 0)
                {
                    Errors.Add("برجاء التاكد من صحه مدخلات الاحجام");
                }

                if (model.OriginalPrices != null && model.OfferPrices != null)
                {
                    for (int i = 0; i < model.OriginalPrices.Count(); i++)
                    {
                        if (model.OfferPrices.ElementAtOrDefault(i) > model.OriginalPrices.ElementAtOrDefault(i))
                        {
                            Errors.Add("برجاء التاكد من صحه مدخلات اسعار الاحجام");
                            break;
                        }
                    }
                }
            }
            return Errors;
        }

        public ActionResult GetCategories(long StoreId)
        {
            var Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == StoreId).ToList();
            return Json(new { Success = true, Data = Categories.Select(s => new { s.Id, s.NameAr }) }, JsonRequestBehavior.AllowGet);
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

        public ActionResult ToggleDelete(long? ProductId, string ReturnUrl)
        {
            if (ProductId.HasValue == true)
            {
                var Product = db.Products.Find(ProductId.Value);
                if (Product != null)
                {
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        if (Product.IsDeleted == true)
                        {
                            CRUD<Product>.Restore(Product);
                        }
                        else
                        {
                            CRUD<Product>.Delete(Product);
                        }
                    }
                    else
                    {
                        if (Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId))
                        {
                            if (Product.IsDeleted == true)
                            {
                                CRUD<Product>.Restore(Product);
                            }
                            else
                            {
                                CRUD<Product>.Delete(Product);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            return Redirect(ReturnUrl);
        }

        [HttpDelete]
        public JsonResult DeleteImage(long ImageId)
        {
            var ProductImage = db.ProductImages.Find(ImageId);
            if (ProductImage != null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    MediaControl.Delete(FilePath.Product, ProductImage.ImageUrl);
                    CRUD<ProductImage>.Delete(ProductImage);
                }
                else
                {
                    if (ProductImage.Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId))
                    {
                        MediaControl.Delete(FilePath.Product, ProductImage.ImageUrl);
                        CRUD<ProductImage>.Delete(ProductImage);
                    }
                }
                db.SaveChanges();
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public JsonResult DeleteSize(long SizeId)
        {
            var ProductSize = db.ProductSizes.Find(SizeId);
            if (ProductSize != null)
            {
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    CRUD<ProductSize>.Delete(ProductSize);
                }
                else
                {
                    if (ProductSize.Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId))
                    {
                        CRUD<ProductSize>.Delete(ProductSize);
                    }
                }
                db.SaveChanges();
                if (ProductSize.Product.Sizes.Any(w => w.IsDeleted == false) == false)
                {
                    ProductSize.Product.IsMultipleSize = false;
                    db.SaveChanges();
                }
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(long? ProductId)
        {
            List<Store> Stores = null;
            if (ProductId.HasValue == true)
            {
                var Product = db.Products.Find(ProductId.Value);
                if (Product != null)
                {
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                        ViewBag.Stores = Stores;
                        ViewBag.Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == Product.Category.StoreId).ToList();
                        ViewBag.Images = Product.Images.Where(s => s.IsDeleted == false).ToList();
                        ViewBag.Sizes = Product.Sizes.Where(s => s.IsDeleted == false).ToList();
                        EditProductVM productVM = EditProductVM.ToEditProductVM(Product);
                        return View(productVM);
                    }
                    else
                    {
                        if (Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId))
                        {
                            Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(x => x.Store).ToList();
                            ViewBag.Stores = Stores;
                            ViewBag.Categories = db.ProductCategories.Where(s => s.IsDeleted == false && s.StoreId == Product.Category.StoreId).ToList();
                            ViewBag.Images = Product.Images.Where(s => s.IsDeleted == false).ToList();
                            ViewBag.Sizes = Product.Sizes.Where(s => s.IsDeleted == false).ToList();
                        EditProductVM productVM = EditProductVM.ToEditProductVM(Product);
                            return View(productVM);
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(EditProductVM model)
        {
            var Errors = ProductEditionValidation(model);
            if (Errors.Count() > 0)
            {
                goto Return;
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var product = db.Products.Find(model.ProductId);
                        product.SubCategoryId = model.CategoryId;
                        product.DescriptionAr = model.DescriptionAr;
                        product.DescriptionEn = model.DescriptionEn;
                        product.NameAr = model.NameAr;
                        product.NameEn = model.NameEn;
                        product.CurrencyAr = model.CurrencyAr;
                        product.Currency = model.Currency;
                        product.Inventory = model.Inventory;
                        if (product.Sizes != null && product.Sizes.Count(s => s.IsDeleted == false) > 0)
                        {
                            product.IsMultipleSize = true;
                            product.SingleOriginalPrice = null;
                            product.SingleOfferPrice = null;
                        }
                        else
                        {
                            product.SingleOriginalPrice = model.SingleOriginalPrice;
                            product.SingleOfferPrice = model.SingleOfferPrice;
                            product.IsMultipleSize = false;
                        }
                        CRUD<Product>.Update(product);
                        db.SaveChanges();
                        if (model.OriginalPrices != null && model.OriginalPrices.Count() > 0)
                        {
                            product.IsMultipleSize = true;
                            for (int i = 0; i < model.OriginalPrices.Count(); i++)
                            {
                                if (string.IsNullOrEmpty(model.SizesAr.ElementAtOrDefault(i)) || string.IsNullOrEmpty(model.SizesEn.ElementAtOrDefault(i)))
                                {
                                    continue;
                                }
                                db.ProductSizes.Add(new ProductSize()
                                {
                                    OfferPrice = model.OfferPrices.ElementAtOrDefault(i) > 0 ? (decimal?)model.OfferPrices.ElementAtOrDefault(i) : null,
                                    OriginalPrice = model.OriginalPrices.ElementAtOrDefault(i),
                                    SizeAr = model.SizesAr.ElementAtOrDefault(i),
                                    SizeEn = model.SizesEn.ElementAtOrDefault(i),
                                    ProductId = product.Id
                                });
                            }
                        }
                        if (model.Images != null)
                        {
                            foreach (var image in model.Images.Where(w => w != null))
                            {
                                db.ProductImages.Add(new ProductImage()
                                {
                                    ImageUrl = MediaControl.Upload(FilePath.Product, image),
                                    ProductId = product.Id
                                });
                            }
                        }
                        db.SaveChanges();
                        Transaction.Commit();
                        return Json(new { Success = true, model.StoreId }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        Errors.Add("برجاء المحاولة فى وقت لاحق");
                        goto Return;
                    }
                }
            }
            Return:
            return Json(new { Success = false, Errors }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Offers(long? StoreId,long? DeletedProduct)
        {
            ViewBag.StoreId = StoreId;
            if (DeletedProduct.HasValue && DeletedProduct.Value == 1)
            {
                ViewBag.Tit = "Done";
                ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                var Products = db.Products.Include(x => x.Category).Include(x => x.Category.Store)
                   .Where(s => (s.SingleOfferPrice.HasValue == true || s.Sizes.Any(x=>x.OfferPrice.HasValue))
                   &&(s.IsDeleted || s.Category.IsDeleted || s.Category.Store.IsDeleted)).OrderByDescending(s => s.CreatedOn).ToList();
                return View(Products);
            }
            if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
            {
                var Products = db.Products.Where(s => (s.SingleOfferPrice.HasValue == true || s.Sizes.Any(x => x.OfferPrice.HasValue && !x.IsDeleted)) &&
                !s.IsDeleted && !s.Category.IsDeleted && !s.Category.Store.IsDeleted
                ).OrderByDescending(s => s.CreatedOn).ToList();
                if (StoreId.HasValue == true)
                {
                    Products = Products.Where(w => w.Category.StoreId == StoreId.Value).ToList();
                }
                ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                return View(Products);
            }
            else if (User.IsInRole("Store"))
            {
                if (StoreId.HasValue == true)
                {
                    var Store = db.StoreUsers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.StoreId == StoreId.Value);
                    if (Store != null)
                    {
                        ViewBag.Store = Store.Store;
                        ViewBag.Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(q => q.Store).ToList();
                        var Products = db.Products.Where(w => w.Category.StoreId == StoreId.Value && 
                        (w.SingleOfferPrice.HasValue == true || w.Sizes.Any(x => x.OfferPrice.HasValue && !x.IsDeleted))).ToList();
                        return View(Products);
                    }
                }
                else
                {
                    ViewBag.Stores = db.StoreUsers.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).Select(q => q.Store).ToList();
                    return View();
                }
            }
            return RedirectToAction("Index", "Cp");
        }
        private List<string> ProductEditionValidation(EditProductVM model)
        {
            List<string> Errors = new List<string>();
            var Product = db.Products.Find(model.ProductId);
            if (Product == null)
            {
                Errors.Add("المنتج المطلوب غير متاح");
            }

            if (User.IsInRole("Store") && Product != null)
            {
                if (Product.Category.Store.Owners.Any(w => w.IsDeleted == false && w.UserId == CurrentUserId) == false)
                {
                    Errors.Add("المنتج المطلوب غير متاح");
                }
            }

            var Store = db.Stores.FirstOrDefault(s => s.IsDeleted == false && s.Id == model.StoreId);
            if (Store == null)
                Errors.Add("المتجر المطلوب غير متاح");

            var Category = db.ProductCategories.FirstOrDefault(s => s.IsDeleted == false && s.Id == model.CategoryId && s.StoreId == model.StoreId);
            if (Category == null)
                Errors.Add("القسم او الفئة المطلوبة غير متاحة");

            if (string.IsNullOrEmpty(model.NameAr) || string.IsNullOrEmpty(model.NameEn))
                Errors.Add("الاسم مطلوب");

            if (model.Images != null)
            {
                foreach (var image in model.Images.Where(w => w != null))
                {
                    if (CheckFiles.IsImage(image) == false)
                    {
                        Errors.Add("احدى الصور غير صحيحة");
                        break;
                    }
                }
            }

            if (Product != null)
            {
                if (Product.Sizes.Any(w => w.IsDeleted == false) == false && Product.SingleOriginalPrice.HasValue == false)
                {
                    if (model.SingleOriginalPrice.HasValue == false)
                    {
                        if (model.OriginalPrices == null || model.OriginalPrices.Count() <= 0)
                        {
                            Errors.Add("برجاء كتابة السعر او اختيار احجام");
                        }
                    }
                    else
                    {
                        if (model.SingleOfferPrice.HasValue == true && model.SingleOfferPrice.Value > model.SingleOriginalPrice.Value)
                            Errors.Add("السعر بعد الخصم غير صحيح");
                    }
                }
            }
            
            if (model.SizesAr != null || model.SizesEn != null || model.OriginalPrices != null || model.OfferPrices != null)
            {
                int SizesArLength = -1;
                if (model.SizesAr != null)
                    SizesArLength = model.SizesAr.Count();

                int SizesEnLength = -1;
                if (model.SizesEn != null)
                    SizesEnLength = model.SizesEn.Count();

                int OriginalPricesLength = -1;
                if (model.OriginalPrices != null)
                    OriginalPricesLength = model.OriginalPrices.Count();

                if (SizesArLength != SizesEnLength || SizesArLength != OriginalPricesLength || SizesEnLength != OriginalPricesLength || SizesEnLength < 0 || SizesArLength < 0 || OriginalPricesLength < 0)
                {
                    Errors.Add("برجاء التاكد من صحه مدخلات الاحجام");
                }

                if (model.OriginalPrices != null && model.OfferPrices != null)
                {
                    for (int i = 0; i < model.OriginalPrices.Count(); i++)
                    {
                        if (model.OfferPrices.ElementAtOrDefault(i) > model.OriginalPrices.ElementAtOrDefault(i))
                        {
                            Errors.Add("برجاء التاكد من صحه مدخلات اسعار الاحجام");
                            break;
                        }
                    }
                }
            }
            return Errors;
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long ProdId, int Number)
        {
            var Product = db.Products.Find(ProdId);
            if (Product == null)
            {
                return Json(new { Sucess = false, Message = "المنتج المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingProductNumber = db.Products.FirstOrDefault(w => w.Id != ProdId && w.SubCategoryId == Product.SubCategoryId && w.SortingNumber == Number);
            if (ExistingProductNumber != null)
            {
                return Json(new { Sucess = false, Message = $"المنتج {ExistingProductNumber.NameAr} له نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            Product.SortingNumber = Number;
            CRUD<Product>.Update(Product);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}