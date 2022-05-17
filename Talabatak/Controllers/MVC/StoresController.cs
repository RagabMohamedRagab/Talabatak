
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin, Store")]
    public class StoresController : BaseController
    {
        public ActionResult Index(long? CatId, string Search = "")
        {
            List<Store> Stores = new List<Store>();
            if (Search == "" || string.IsNullOrEmpty(Search)  || Search == "3")
            {
                Stores = db.Stores.ToList();
            }
            else if(Search == "1")
            {
                Stores = db.Stores.Where(w => w.Wallet > 0).ToList();
            }
            else
            {
                Stores = db.Stores.Where(w => w.Wallet < 0).ToList();

            }
            return View(Stores);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.Where(d => d.IsDeleted == false), "Id", "NameAr");
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateStoreVM model)
        {
            return await RegisterTheStore(model, ModelState);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Join(CreateStoreVM model)
        {
            return await RegisterTheStore(model, ModelState);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Join()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.Where(d => d.IsDeleted == false), "Id", "NameAr");
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Success()
        {
            if (TempData["RegisterSuccess"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Join");
            }
        }

        private async Task<ActionResult> RegisterTheStore(CreateStoreVM model, ModelStateDictionary ModelState)
        {
            var ValidateCreation = ValidateCreate(model, ModelState);
            if (ValidateCreation.IsValid)
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        TimeSpan? OpenFrom = null;
                        TimeSpan? OpenTo = null;
                        if (!string.IsNullOrEmpty(model.From) && !string.IsNullOrEmpty(model.To))
                        {
                            OpenFrom = DateTime.Parse(model.From).TimeOfDay;
                            OpenTo = DateTime.Parse(model.To).TimeOfDay;
                        }
                        Store store = new Store()
                        {
                            CategoryId = model.CategoryId,
                            CityId = model.CityId,
                            DescriptionAr = model.DescriptionAr,
                            DescriptionEn = model.DescriptionEn,
                            LogoImageUrl = MediaControl.Upload(FilePath.Store, model.StoreLogo),
                            CoverImageUrl = MediaControl.Upload(FilePath.Store, model.CoverImage),
                            TaxReportImageUrl= MediaControl.Upload(FilePath.Store, model.TaxReportImage),
                            Is24HourOpen = model.Is24HourOpen,
                            IsOpen = model.Is24HourOpen ? true : false,
                            NameAr = model.NameAr,
                            NameEn = model.NameEn,
                            OpenFrom = OpenFrom,
                            OpenTo = OpenTo,
                            TaxReportNumber=model.TaxReportNumber,
                            OfficialEmail=model.OfficialEmail,
                            ValueAddedTax=model.ValueAddedTax,
                            AddressAr = model.AddressAr,
                            AddressEn = model.AddressEn,
                            IsClosingManual = false,
                            PhoneNumber = model.PhoneNumber,
                            ResponsiblePersonName = model.OwnerName,
                            ResponsiblePersonPhoneNumber = model.OwnerPhoneNumber,
                            ResponsiblePersonEmail = model.OwnerEmail,
                            Profit=(double) model.Profit,
                            DeliveryBySystem=model.DeliveryBySystem,
                            StoreOrdersDeliveryEveryKilometerPrice=model.StoreOrdersDeliveryEveryKilometerPrice,
                            StoreOrdersDeliveryOpenFarePrice=model.StoreOrdersDeliveryOpenFarePrice,
                            StoreOrdersDeliveryOpenFareKilometers=model.StoreOrdersDeliveryOpenFareKilometers
                        };
                        if (model.ValueAddedTaxImage != null)
                        {
                            store.ValueAddedTaxImageUrl = MediaControl.Upload(FilePath.Store, model.ValueAddedTaxImage);
                        }
                        if (model.TaxImage != null)
                        {
                            store.TaxImageUrl = MediaControl.Upload(FilePath.Store, model.TaxImage);
                        }
                        if (User.Identity.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SubAdmin")))
                        {
                            store.Latitude = model.Latitude;
                            store.Longitude = model.Longitude;
                            store.IsAccepted = true;
                            store.AcceptedOn = DateTime.Now.ToUniversalTime();
                        }
                        db.Stores.Add(store);
                        db.SaveChanges();
                        Transaction.Commit();
                        if (User.Identity.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SubAdmin")))
                            return RedirectToAction("Index");
                        else
                        {
                            TempData["RegisterSuccess"] = true;
                            Notifications.SendWebNotification($"المتجر {store.NameAr} يريد الانضمام", $"المتجر {store.NameAr} يريد الانضمام", Role: "Admin", Id: store.Id, notificationLinkType: NotificationType.AdminStoresPage, IsSaveInDatabase: true);
                            return RedirectToAction("Success");
                        }
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    }
                }
            }
            ViewBag.CategoryId = new SelectList(db.Categories.Where(d => d.IsDeleted == false), "Id", "NameAr", model.CategoryId);
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        private ModelStateDictionary ValidateCreate(CreateStoreVM model, ModelStateDictionary ModelState)
        {
            var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
            if (City == null)
                ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

            var Category = db.Categories.Any(d => d.Id == model.CategoryId && d.IsDeleted == false);
            if (Category == false)
                ModelState.AddModelError("CategoryId", "القسم المطلوب غير متاح");

            if (model.StoreLogo != null)
            {
                bool IsImage = CheckFiles.IsImage(model.StoreLogo);
                if (IsImage == false)
                    ModelState.AddModelError("StoreLogo", "شعار المتجر غير صحيح");
            }
            else
                ModelState.AddModelError("StoreLogo", "شعار المتجر مطلوب");

            if (model.CoverImage != null)
            {
                bool IsImage = CheckFiles.IsImage(model.CoverImage);
                if (IsImage == false)
                    ModelState.AddModelError("CoverImage", "صوره غلاف المتجر غير صحيحة");
            }
            if (model.Is24HourOpen == false)
            {
                if (string.IsNullOrEmpty(model.From))
                    ModelState.AddModelError("From", "موعد المتجر من الساعه .. مطلوب");
                else
                {
                    try
                    {
                        TimeSpan TimeFrom = DateTime.Parse(model.From).TimeOfDay;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("From", "التوقيت من غير صحيح");
                    }
                }

                if (string.IsNullOrEmpty(model.To))
                    ModelState.AddModelError("To", "موعد المتجر إلى الساعه .. مطلوب");
                else
                {
                    try
                    {
                        TimeSpan TimeTo = DateTime.Parse(model.To).TimeOfDay;
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("To", "التوقيت إلى غير صحيح");
                    }
                }
            }
            else
            {
                if (ModelState.ContainsKey("From"))
                    ModelState["From"].Errors.Clear();

                if (ModelState.ContainsKey("To"))
                    ModelState["To"].Errors.Clear();
            }
            

            if (model.PhoneNumber == null)
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
            }

            if (User.IsInRole("Admin") == false && User.IsInRole("SubAdmin") == false)
            {
                if (string.IsNullOrEmpty(model.OwnerName))
                {
                    ModelState.AddModelError("OwnerName", "اسم المسؤول مطلوب");
                }
                if (string.IsNullOrEmpty(model.OwnerPhoneNumber))
                {
                    ModelState.AddModelError("OwnerPhoneNumber", "رقم هاتف المسؤول مطلوب");
                }
                if (string.IsNullOrEmpty(model.OwnerEmail))
                {
                    ModelState.AddModelError("OwnerEmail", "بريد المسؤول مطلوب");
                }

            }
            else
            {
                if (model.Latitude.HasValue == false || model.Longitude.HasValue == false)
                {
                    ModelState.AddModelError("Latitude", "الموقع على الخريطه مطلوب");
                }
                if (ModelState.ContainsKey("OwnerName"))
                    ModelState["OwnerName"].Errors.Clear();
                if (ModelState.ContainsKey("OwnerPhoneNumber"))
                    ModelState["OwnerPhoneNumber"].Errors.Clear();
                if (ModelState.ContainsKey("OwnerEmail"))
                    ModelState["OwnerEmail"].Errors.Clear();
            }
            return ModelState;
        }

        public ActionResult Users(long? StoreId)
        {
            if (StoreId.HasValue == true)
            {
                var Store = db.Stores.Find(StoreId.Value);
                if (Store != null)
                {
                    return View(Store);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult CreateUser(long? StoreId)
        {
            if (StoreId.HasValue == true)
            {
                var Store = db.Stores.Find(StoreId.Value);
                if (Store != null)
                {
                    ViewBag.Store = Store;
                    ViewBag.Users = db.Users.ToList();
                    ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser(CreateStoreUserVM model)
        {
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var Store = db.Stores.Find(model.StoreId);

            ValidateUserCreation(model);

            if (ModelState.IsValid == false)
            {
                goto Return;
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        ApplicationUser storeUser = null;
                        if (string.IsNullOrEmpty(model.UserId) || model.UserId == "-1")
                        {
                            string ImageName = null;
                            if (model.Image != null)
                            {
                                ImageName = MediaControl.Upload(FilePath.Users, model.Image);
                            }
                            var user = new ApplicationUser()
                            {
                                PhoneNumberConfirmed = true,
                                EmailConfirmed = true,
                                ImageUrl = ImageName,
                                UserName = model.Email,
                                Email = model.Email,
                                Name = model.Name,
                                PhoneNumber = model.PhoneNumber,
                                VerificationCode = 1111,
                                CityId = model.CityId
                            };
                            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                            if (result.Succeeded == false)
                            {
                                if (model.Image != null)
                                {
                                    MediaControl.Delete(FilePath.Users, ImageName);
                                }
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                                goto Return;
                            }
                            storeUser = user;
                        }
                        else
                        {
                            storeUser = db.Users.Find(model.UserId);
                        }
                        if (UserManager.IsInRole(storeUser.Id, "Store") == false)
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(storeUser.Id, "Store");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                                goto Return;
                            }
                        }
                        var CurrentStoreUser = db.StoreUsers.FirstOrDefault(s => s.StoreId == Store.Id && s.UserId == storeUser.Id);
                        if (CurrentStoreUser != null)
                        {
                            if (CurrentStoreUser.IsDeleted == true)
                            {
                                CRUD<StoreUser>.Restore(CurrentStoreUser);
                            }
                        }
                        else
                        {
                            StoreUser UserObject = new StoreUser()
                            {
                                StoreId = Store.Id,
                                UserId = storeUser.Id
                            };
                            db.StoreUsers.Add(UserObject);
                        }
                        db.SaveChanges();
                        Transaction.Commit();
                        return RedirectToAction("Users", new { StoreId = Store.Id });
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                        goto Return;
                    }
                }
            }
            Return:
            ViewBag.Store = Store;
            ViewBag.Users = db.Users.ToList();
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View();
        }

        private void ValidateUserCreation(CreateStoreUserVM model)
        {
            if (string.IsNullOrEmpty(model.UserId) || model.UserId == "-1")
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", "اسم المستخدم مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (string.IsNullOrEmpty(model.PhoneNumber))
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                }

                if (string.IsNullOrEmpty(model.Password))
                    ModelState.AddModelError("Password", "كلمة السر مطلوبة");

                if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 6)
                    ModelState.AddModelError("Password", "كلمة السر يجب ان لا تقل عن 6 أحرف");

                if (string.IsNullOrEmpty(model.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "تأكيد كلمة السر مطلوبة");

                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "كلمة السر غير متطابقة");

                if (string.IsNullOrEmpty(model.Email))
                    ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");

                if (model.Image != null)
                {
                    bool IsImageValid = CheckFiles.IsImage(model.Image);
                    if (IsImageValid == false)
                        ModelState.AddModelError("Image", "صوره المسؤول غير صحيحة");
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var IsEmailExist = db.Users.Any(d => d.Email.ToLower() == model.Email.ToLower());
                    if (IsEmailExist == true)
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                    }
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var IsPhoneExist = db.Users.Any(d => d.PhoneNumber.ToLower() == model.PhoneNumber.ToLower());
                    if (IsPhoneExist == true)
                    {
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                    }
                }
            }
            else
            {

                var CurrentUser = db.Users.Find(model.UserId);
                if (CurrentUser == null)
                    ModelState.AddModelError("UserId", "العضو المطلوب غير متاح");

                if (ModelState.ContainsKey("Name"))
                    ModelState["Name"].Errors.Clear();

                if (ModelState.ContainsKey("Email"))
                    ModelState["Email"].Errors.Clear();

                if (ModelState.ContainsKey("PhoneNumber"))
                    ModelState["PhoneNumber"].Errors.Clear();

                if (ModelState.ContainsKey("CityId"))
                    ModelState["CityId"].Errors.Clear();

                if (ModelState.ContainsKey("Password"))
                    ModelState["Password"].Errors.Clear();

                if (ModelState.ContainsKey("ConfirmPassword"))
                    ModelState["ConfirmPassword"].Errors.Clear();
            }
        }

        public ActionResult ToggleDeleteUser(long Id)
        {
            var StoreUser = db.StoreUsers.Find(Id);
            if (StoreUser != null)
            {
                if (StoreUser.IsDeleted == true)
                {
                    CRUD<StoreUser>.Restore(StoreUser);
                }
                else
                {
                    CRUD<StoreUser>.Delete(StoreUser);
                }
                db.SaveChanges();
                return RedirectToAction("Users", new { StoreId = StoreUser.StoreId });
            }
            return RedirectToAction("Users");
        }
        [HttpGet]
        public ActionResult Edit(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id);
            if (Store == null)
                return RedirectToAction("Index");

            ViewBag.Categories = db.Categories.Where(d => d.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();

            EditStoreVM storeVM = new EditStoreVM()
            {
                CategoryId = Store.CategoryId,
                DescriptionAr = Store.DescriptionAr,
                DescriptionEn = Store.DescriptionEn,
                OldFrom = Store.OpenFrom,
                OldTo = Store.OpenTo,
                AddressAr = Store.AddressAr,
                AddressEn = Store.AddressEn,
                Is24HourOpen = Store.Is24HourOpen,
                Latitude = Store.Latitude,
                Longitude = Store.Longitude,
                NameAr = Store.NameAr,
                NameEn = Store.NameEn,
                Id = Store.Id,
                CityId = Store.CityId,
                PhoneNumber = Store.PhoneNumber,
                DeliveryBySystem=Store.DeliveryBySystem,
                StoreOrdersDeliveryEveryKilometerPrice=Store.StoreOrdersDeliveryEveryKilometerPrice,
                StoreOrdersDeliveryOpenFareKilometers=Store.StoreOrdersDeliveryOpenFareKilometers,
                StoreOrdersDeliveryOpenFarePrice=Store.StoreOrdersDeliveryOpenFarePrice
            };
            return View(storeVM);
        }

        private ModelStateDictionary ValidateEdit(EditStoreVM model, ModelStateDictionary ModelState)
        {
            var Store = db.Stores.Find(model.Id);
            if (Store == null)
                ModelState.AddModelError("", "المتجر المطلوب غير متاح");

            var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.Country.IsDeleted == false && d.IsDeleted == false);
            if (City == null)
                ModelState.AddModelError("CityId", "المنقطة المطلوبة غير متاحه");

            var Category = db.Categories.Any(d => d.Id == model.CategoryId && d.IsDeleted == false);
            if (Category == false)
                ModelState.AddModelError("CategoryId", "القسم المطلوب غير متاح");

            if (model.StoreLogo != null)
            {
                bool IsImage = CheckFiles.IsImage(model.StoreLogo);
                if (IsImage == false)
                    ModelState.AddModelError("StoreLogo", "شعار المتجر غير صحيح");
            }

            if (model.CoverImage != null)
            {
                bool IsImage = CheckFiles.IsImage(model.CoverImage);
                if (IsImage == false)
                    ModelState.AddModelError("StoreLogo", "غلاف المتجر غير صحيح");
            }

            if (model.Is24HourOpen == false)
            {
                if (string.IsNullOrEmpty(model.From))
                    ModelState.AddModelError("From", "موعد المتجر من الساعه .. مطلوب");
                else
                {
                    try
                    {
                        TimeSpan TimeFrom = DateTime.Parse(model.From).TimeOfDay;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("From", "التوقيت من غير صحيح");
                    }
                }

                if (string.IsNullOrEmpty(model.To))
                    ModelState.AddModelError("To", "موعد المتجر إلى الساعه .. مطلوب");
                else
                {
                    try
                    {
                        TimeSpan TimeTo = DateTime.Parse(model.To).TimeOfDay;
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("To", "التوقيت إلى غير صحيح");
                    }
                }
            }
            else
            {
                if (ModelState.ContainsKey("From"))
                    ModelState["From"].Errors.Clear();

                if (ModelState.ContainsKey("To"))
                    ModelState["To"].Errors.Clear();
            }

            if (model.Latitude.HasValue == false || model.Longitude.HasValue == false)
            {
                ModelState.AddModelError("Latitude", "الموقع على الخريطه مطلوب");
            }

            if (model.PhoneNumber == null)
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
            }
           
            return ModelState;
        }

        [HttpPost]
        public ActionResult Edit(EditStoreVM model)
        {
            var ValidateCreation = ValidateEdit(model, ModelState);
            if (ValidateCreation.IsValid)
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var Store = db.Stores.Find(model.Id);
                        Store.NameAr = model.NameAr;
                        Store.NameEn = model.NameEn;
                        Store.DescriptionAr = model.DescriptionAr;
                        Store.DescriptionEn = model.DescriptionEn;
                        Store.CategoryId = model.CategoryId;
                        Store.Latitude = model.Latitude;
                        Store.Longitude = model.Longitude;
                        Store.AddressAr = model.AddressAr;
                        Store.AddressEn = model.AddressEn;
                        Store.PhoneNumber = model.PhoneNumber;
                        Store.CityId = model.CityId;
                        Store.Profit = (double) model.Profit;
                        Store.DeliveryBySystem = model.DeliveryBySystem;
                        Store.StoreOrdersDeliveryEveryKilometerPrice = model.StoreOrdersDeliveryEveryKilometerPrice;
                        Store.StoreOrdersDeliveryOpenFareKilometers = model.StoreOrdersDeliveryOpenFareKilometers;
                        Store.StoreOrdersDeliveryOpenFarePrice = model.StoreOrdersDeliveryOpenFarePrice;
                        if (model.Is24HourOpen == false && !string.IsNullOrEmpty(model.From) && !string.IsNullOrEmpty(model.To))
                        {
                            Store.Is24HourOpen = false;
                            Store.OpenFrom = DateTime.Parse(model.From).TimeOfDay;
                            Store.OpenTo = DateTime.Parse(model.To).TimeOfDay;
                        }
                        else
                        {
                            Store.Is24HourOpen = true;
                            Store.IsOpen = true;
                            Store.OpenFrom = null;
                            Store.OpenTo = null;
                        }
                        if (model.StoreLogo != null)
                        {
                            if (!string.IsNullOrEmpty(Store.LogoImageUrl))
                            {
                                MediaControl.Delete(FilePath.Store, Store.LogoImageUrl);
                            }
                            Store.LogoImageUrl = MediaControl.Upload(FilePath.Store, model.StoreLogo);
                        }
                        if (model.CoverImage != null)
                        {
                            if (!string.IsNullOrEmpty(Store.CoverImageUrl))
                            {
                                MediaControl.Delete(FilePath.Store, Store.CoverImageUrl);
                            }
                            Store.CoverImageUrl = MediaControl.Upload(FilePath.Store, model.CoverImage);
                        }
                        db.SaveChanges();
                        Transaction.Commit();
                        return RedirectToAction("Details", new { Store.Id });
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    }
                }
            }
            ViewBag.Categories = db.Categories.Where(d => d.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        [HttpGet]
        public ActionResult Details(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id);
            if (Store == null)
                return RedirectToAction("Index");

            return View(Store);
        }

        [HttpGet]
        public ActionResult Control(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id);
            if (Store == null)
                return RedirectToAction("Index");

            return View(Store);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Accept(long Id)
        {
            var Store = db.Stores.Find(Id);
            if (Store != null)
            {
                Store.IsAccepted = true;
                Store.AcceptedOn = DateTime.Now.ToUniversalTime();
                TempData["AcceptSuccess"] = true;
                db.SaveChanges();
            }
            return RedirectToAction("Control", new { Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(long Id, string RejectionReason)
        {
            var Store = db.Stores.Find(Id);
            if (Store != null)
            {
                Store.IsAccepted = false;
                Store.RejectedOn = DateTime.Now.ToUniversalTime();
                Store.RejectionReason = RejectionReason;
                TempData["RejectSuccess"] = true;
                db.SaveChanges();
            }
            return RedirectToAction("Control", new { Id });
        }

        [HttpGet]
        public ActionResult ToggleDelete(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id.Value);
            if (Store == null || Store.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Store.IsDeleted == true)
            {
                CRUD<Store>.Restore(Store);
            }
            else
            {
                CRUD<Store>.Delete(Store);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { Id });
        }

        [HttpGet]
        public ActionResult ToggleHide(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id.Value);
            if (Store == null || Store.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Store.IsHidden == true)
            {
                if (Store.IsBlocked == false)
                {
                    Store.IsHidden = false;
                    CRUD<Store>.Update(Store);
                }
            }
            else
            {
                Store.IsHidden = true;
                CRUD<Store>.Update(Store);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { Id });
        }

        [HttpGet]
        public ActionResult ToggleStop(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Store = db.Stores.Find(Id.Value);
            if (Store == null || Store.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Store.IsBlocked == true)
            {
                Store.IsBlocked = false;
                CRUD<Store>.Update(Store);
            }
            else
            {
                Store.IsBlocked = true;
                Store.IsHidden = true;
                CRUD<Store>.Update(Store);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { Id });
        }

        [HttpGet]
        public ActionResult MustPayToStores()
        {
            var Stores = db.Stores.Where(s => s.Wallet >= 100).ToList();
            return View(Stores);
        }
        [HttpPost]
        public JsonResult SetSortingNumber(long StoreId, int Number)
        {
            var Store = db.Stores.Find(StoreId);
            if (Store == null)
            {
                return Json(new { Sucess = false, Message = "المنتج المطلوب غير متوفر" }, JsonRequestBehavior.AllowGet);
            }
            Store.SortingNumber = Number;
            CRUD<Store>.Update(Store);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            return View(
                new ResetPasswordVM()
                {
                    Id = id,
                }
           );
        }
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            /*if (!ModelState.IsValid)
            {
                return BadRequest();
            }*/
       /*     var user = db.Users.FirstOrDefault(x => x.Id == resetPasswordVM.Id);
            user.PasswordHash*/


            if (ModelState.IsValid == true)
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var user = db.Users.FirstOrDefault(x => x.Id == resetPasswordVM.Id);
                if (user != null)
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(resetPasswordVM.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    return RedirectToAction(nameof(Index));
                }
                
            }

            return RedirectToAction(nameof(Index));

        }
        public async Task<ActionResult> Report(long? id)
        {
            StoreReportVM model = new StoreReportVM();
            if (!id.HasValue)
            {
                return View(nameof(Index));
            }
            if (id.HasValue == true)
            {
                if (!await db.Stores.AnyAsync(x => x.Id == id.Value))
                {
                    return View(nameof(Index));
                }
                model.StoreOrders = new List<StoreOrder>();
                model.ForSystem = (decimal)0.0;
                model.Store = await db.Stores.Include(x => x.Category).Include(x => x.City).Include(x => x.City.Country).Include(x => x.ProductCategories).Include(x => x.Reviews).FirstOrDefaultAsync(x => x.Id == id.Value);
                model.OwnersCount = (await db.StoreUsers.Where(x => x.StoreId == id).ToListAsync()).Count;
                model.ProductCategoriesCount = model.Store.ProductCategories.Count;
                List<Product> products = new List<Product>();
                foreach (var item in model.Store.ProductCategories.ToList())
                {
                    if (db.Products.Any(x => x.SubCategoryId == item.Id))
                    {
                        model.ProductCount += db.Products.Where(x => x.SubCategoryId == item.Id).ToList().Count;
                    }
                }
                model.StoreReviews = db.StoreReviews.Include(x=>x.User).Where(x => x.StoreId == id).ToList();
             }
            return View(model);
        }
    }
}