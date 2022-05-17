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
using System.Data.Entity.Core.Objects;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]

    public class DriversController : BaseController
    {
        public ActionResult Index(string q)
        {
            IQueryable<Driver> Drivers = db.Drivers.Include(d => d.Orders).Include(d => d.User).Include(d=>d.City);
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "new")
            {
                Drivers = Drivers.Where(s => s.IsAccepted.HasValue == false).OrderByDescending(s => s.CreatedOn);
                goto Return;
            }
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "rejected")
            {
                Drivers = Drivers.Where(s => s.IsAccepted == false).OrderByDescending(s => s.CreatedOn);
                goto Return;
            }
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "accepted")
            {
                Drivers = Drivers.Where(s => s.IsAccepted == true).OrderByDescending(s => s.CreatedOn);
                goto Return;
            }
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "available")
            {
                Drivers = Drivers.Where(s => s.IsAccepted == true && s.IsBlocked == false && s.IsOnline == true && s.IsDeleted == false && s.IsAvailable == true).OrderByDescending(s => s.CreatedOn);
                goto Return;
            }
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "blocked")
            {
                Drivers = Drivers.Where(s => s.IsBlocked == true).OrderByDescending(s => s.CreatedOn);
                goto Return;
            }
            Return:

            List<DriverIndexVM> model = new List<DriverIndexVM>();
            decimal total;
            foreach (var driver in Drivers.ToList())
            {
                total = (decimal)0.0;
                if (db.OtlobAy7agaOrders.Any(x => x.DriverId == driver.Id && x.OrderStatus == OtlobAy7agaOrderStatus.Finished))
                {
                    total += db.OtlobAy7agaOrders.Where(x => x.DriverId == driver.Id && x.OrderStatus == OtlobAy7agaOrderStatus.Finished).Sum(x => x.DeliveryFees);
                }
                if (db.StoreOrders.Any(x => x.DriverId == driver.Id && x.Status == StoreOrderStatus.Finished))
                {
                    total += db.StoreOrders.Where(x => x.DriverId == driver.Id && x.Status == StoreOrderStatus.Finished).Sum(x => x.DeliveryFees);
                }
                model.Add(new DriverIndexVM()
                {
                    Driv = driver,
                    Paid = db.UserWallets.Any(x => x.UserId == driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually) ?
                       db.UserWallets.Where(x => x.UserId == driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually)
                       .Sum(x => x.TransactionAmount)
                       : (decimal)0.0,
                    Total = total * ((decimal)driver.Profit / (decimal)100)
                });
            }
            return View(model);

        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Join()
        {
            ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(x => !x.IsDeleted && x.Country.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Join(CreateDriverVM model)
        {
            var Validation = ValidateDriverCreation(model, ModelState);
            if (Validation.IsValid == false)
            {
                goto Jump;
            }

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string Email = "Driver_" + RandomGenerator.GenerateString(8) + "@Talabatak.com";
                    var user = new ApplicationUser()
                    {
                        CreatedOn = DateTime.Now.ToUniversalTime(),
                        PhoneNumberConfirmed = true,
                        EmailConfirmed = true,
                        ImageUrl = MediaControl.Upload(FilePath.Users, model.PersonalPhoto),
                        UserName = Email,
                        Email = Email,
                        CityId = model.CityId,
                        Name = model.Name,
                        RegisterationType = RegisterationType.Internal,
                        PhoneNumber = model.PhoneNumber,
                        VerificationCode = 1111,
                    };
                    IdentityResult result = await UserManager.CreateAsync(user, Email);
                    if (result.Succeeded == false)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                        goto Jump;
                    }
                    else
                    {
                        IdentityResult RoleResult = UserManager.AddToRole(user.Id, "Driver");
                        if (RoleResult.Succeeded == false)
                        {
                            Transaction.Rollback();
                            ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                            goto Jump;
                        }
                    }

                    Driver driver = new Driver()
                    {
                        IsOnline = false,
                        IsBlocked = false,
                        IsAvailable = true,
                        UserId = user.Id,
                        NumberOfCompletedTrips = 0,
                        Rate = 0,
                        VehicleTypeId = model.VehicleTypeId,
                        LicensePhoto = MediaControl.Upload(FilePath.Users, model.LicensePhoto),
                        VehicleLicensePhoto = MediaControl.Upload(FilePath.Users, model.VehicleLicensePhoto),
                        PersonalPhoto = MediaControl.Upload(FilePath.Users, model.PersonalPhoto),
                        IdentityPhoto = MediaControl.Upload(FilePath.Users, model.IdentityPhoto),
                        VehicleColor = model.VehicleColor,
                        VehicleNumber = model.VehicleNumber
                    };
                    db.Drivers.Add(driver);
                    db.SaveChanges();
                    Transaction.Commit();
                    TempData["RegisterSuccess"] = true;
                    return RedirectToAction("Success");

                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاولة لاحقاً");
                    goto Jump;
                }
            }

            Jump:
            ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
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

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
            ViewBag.Users = db.Users.Where(d => d.Drivers.Any(s => s.UserId == d.Id) == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateDriverVM model)
        {
            var Validation = ValidateDriverCreation(model, ModelState);
            if (Validation.IsValid == false)
            {
                goto Jump;
            }

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            bool isValidGuid = false;
            try
            {
                Guid guid = new Guid();
                isValidGuid = Guid.TryParse(model.UserId, out guid);
            }
            catch (Exception)
            {
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string UserId = null;
                    if (isValidGuid == true)
                    {
                        UserId = model.UserId;
                        if (UserManager.IsInRole(model.UserId, "Driver") == false)
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(model.UserId, "Driver");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                                goto Jump;
                            }
                        }
                    }
                    else
                    {
                        var city = db.Cities.Find(model.CityId);
                        var user = new ApplicationUser()
                        {
                            CreatedOn = DateTime.Now.ToUniversalTime(),
                            PhoneNumberConfirmed = true,
                            EmailConfirmed = true,
                            ImageUrl = MediaControl.Upload(FilePath.Users, model.PersonalPhoto),
                            UserName = model.Email,
                            Email = model.Email,
                            CityId = model.CityId,
                            Name = model.Name,
                            RegisterationType = RegisterationType.Internal,
                            PhoneNumber = model.PhoneNumber,
                            VerificationCode = 1111,
                        };
                        ViewBag.CityId = model.CityId;
                        IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded == false)
                        {
                            Transaction.Rollback();
                            ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                            goto Jump;
                        }
                        else
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(user.Id, "Driver");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                                goto Jump;
                            }
                        }
                        UserId = user.Id;
                    }

                    Driver driver = new Driver()
                    {
                        IsOnline = false,
                        IsBlocked = false,
                        IsAvailable = true,
                        UserId = UserId,
                        NumberOfCompletedTrips = 0,
                        Rate = 0,
                        VehicleTypeId = model.VehicleTypeId,
                        LicensePhoto = MediaControl.Upload(FilePath.Users, model.LicensePhoto),
                        VehicleLicensePhoto = MediaControl.Upload(FilePath.Users, model.VehicleLicensePhoto),
                        PersonalPhoto = MediaControl.Upload(FilePath.Users, model.PersonalPhoto),
                        IdentityPhoto = MediaControl.Upload(FilePath.Users, model.IdentityPhoto),
                        VehicleColor = model.VehicleColor,
                        VehicleNumber = model.VehicleNumber,
                        Profit = model.Profit
                    };

                    driver.CityId = model.CityId;
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        driver.AcceptedOn = DateTime.Now.ToUniversalTime();
                        driver.IsAccepted = true;
                    }
                    db.Drivers.Add(driver);
                    db.SaveChanges();
                    Transaction.Commit();
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        TempData["Success"] = true;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["RegisterSuccess"] = true;
                        return RedirectToAction("Success");
                    }

                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    goto Jump;
                }
            }

            Jump:
            ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
            ViewBag.Users = db.Users.Where(d => d.Drivers.Any(s => s.UserId == d.Id) == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        private ModelStateDictionary ValidateDriverCreation(CreateDriverVM model, ModelStateDictionary ModelState)
        {
            if (model.IdentityPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.IdentityPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("IdentityPhoto", "صوره هوية السائق غير صحيحة");
            }

            if (model.LicensePhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.LicensePhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("LicensePhoto", "صوره رخصة السائق غير صحيحة");
            }

            if (model.PersonalPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.PersonalPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("PersonalPhoto", "صوره السائق الشخصية غير صحيحة");
            }

            if (model.VehicleLicensePhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.VehicleLicensePhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("VehicleLicensePhoto", "صوره رخصه المركبة غير صحيحة");
            }

            if (model.VehicleTypeId.HasValue == true)
            {
                var VehicleType = db.VehicleTypes.FirstOrDefault(w => w.IsDeleted == false && w.Id == model.VehicleTypeId);
                if (VehicleType == null)
                {
                    ModelState.AddModelError("VehicleTypeId", "نوع المركبة غير صحيح");
                }
            }

            bool isValidGuid = false;
            try
            {
                Guid guid = new Guid();
                isValidGuid = Guid.TryParse(model.UserId, out guid);
            }
            catch (Exception)
            {
            }

            if (isValidGuid == true)
            {
                var user = db.Users.Find(model.UserId);
                if (user == null)
                {
                    ModelState.AddModelError("UserId", "العضو المطلوب غير متاح");
                }
                else
                {
                    var Driver = db.Drivers.FirstOrDefault(d => d.UserId == user.Id);
                    if (Driver != null)
                    {
                        ModelState.AddModelError("UserId", "هذا العضو مسجل كسائق من قبل");
                    }
                }
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
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("FirstName", "اسم السائق مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (City != null && (City.Country.NameEn.ToLower() == "egypt" || City.Country.NameAr.ToLower() == "مصر"))
                    {
                        string EgyptPhoneNumberSyntax = @"^01[0-2|5]{1}[0-9]{8}";
                        bool IsPhoneNumber = Regex.IsMatch(model.PhoneNumber, EgyptPhoneNumberSyntax, RegexOptions.IgnoreCase);
                        if (IsPhoneNumber == true)
                        {
                            if (model.PhoneNumber.Length != 11)
                            {
                                ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                        }
                    }
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                    else
                    {
                        var user = db.Users.Any(d => d.PhoneNumber.ToLower() == model.PhoneNumber.ToLower());
                        if (user == true)
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
                }

                if (User.IsInRole("Admin") == true || User.IsInRole("SubAdmin") == true)
                {
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var user = db.Users.Any(d => d.Email.ToLower() == model.Email.ToLower());
                        if (user == true)
                        {
                            ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");
                    }
                    if (string.IsNullOrEmpty(model.Password))
                        ModelState.AddModelError("Password", "كلمه السر مطلوبة");

                    if (string.IsNullOrEmpty(model.ConfirmPassword))
                        ModelState.AddModelError("ConfirmPassword", "تأكيد كلمه السر مطلوبة");

                    if (model.Password != model.ConfirmPassword)
                        ModelState.AddModelError("Password", "كلمه السر غير متطابقة");
                }
                else
                {
                    if (ModelState.ContainsKey("Email"))
                        ModelState["Email"].Errors.Clear();
                    if (ModelState.ContainsKey("Password"))
                        ModelState["Password"].Errors.Clear();
                    if (ModelState.ContainsKey("ConfirmPassword"))
                        ModelState["ConfirmPassword"].Errors.Clear();
                }
            }

            return ModelState;
        }
        public async Task<ActionResult> Report(long? DriverId , DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            DriverReportVM model = new DriverReportVM();
            if (!DriverId.HasValue)
            {
                return View(nameof(Index));
            }
            if (DriverId.HasValue == true)
            {
                if (! await db.Drivers.AnyAsync(x => x.Id == DriverId.Value))
                {
                    return View(nameof(Index));
                }
            }
            model.Total = 0;
            model.Driver = await db.Drivers.Include(x => x.User).Include(x=>x.User.City).Include(x=>x.User.City.Country).FirstOrDefaultAsync(x => x.Id == DriverId.Value);

                if (db.OtlobAy7agaOrders.Any(x => x.DriverId == DriverId && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
               (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value,x.CreatedOn)<=0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >=0 : true)))
                {
                    model.Total = db.OtlobAy7agaOrders.Where(x => x.DriverId == DriverId && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
                   (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true )&&
                    (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                        .Sum(x => x.DeliveryFees);
                }
            if (db.StoreOrders.Any(x => x.DriverId == DriverId && x.Status == StoreOrderStatus.Finished
            && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true )&&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
                {
                model.Total += db.StoreOrders.Where(x => x.DriverId == DriverId && x.Status == StoreOrderStatus.Finished
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true )&&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.DeliveryFees);
                }
            model.Paid = 0;
            if (db.UserWallets.Any(x => x.UserId == model.Driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid += db.UserWallets.Where(x => x.UserId == model.Driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            else if (db.UserWallets.Any(x => x.UserId == model.Driver.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                      && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                        (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid -= db.UserWallets.Where(x => x.UserId == model.Driver.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            model.StoreOrders = new List<StoreOrder>();
            if(db.StoreOrders.Any(x => x.DriverId == DriverId
            && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true )&&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.StoreOrders = await db.StoreOrders.Where(x => x.DriverId == DriverId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true )&&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            model.OtlobAy7AgaOrders = new List<OtlobAy7agaOrder>();
            if (db.OtlobAy7agaOrders.Any(x => x.DriverId == DriverId
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.OtlobAy7AgaOrders = await db.OtlobAy7agaOrders.Where(x => x.DriverId == DriverId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.StoreOrders.Where(x => (x.DriverId == DriverId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == StoreOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            foreach (var Order in db.OtlobAy7agaOrders.Where(x => (x.DriverId == DriverId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            model.Stil = model.ForSystem - model.Paid;
            model.Done = (model.OtlobAy7AgaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).Count()) +
                      (model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).Count());
            model.Cancel = (model.OtlobAy7AgaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Cancelled).Count()) +
            (model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Cancelled).Count());

            model.Reject = model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Rejected).Count();
            ViewBag.Title = $"تقرير السائق {model.Driver.User.Name}";
            model.driverReviews = db.DriverReviews.Include(x => x.User).Where(x => x.DriverId == model.Driver.Id).ToList();
            return View(model);
        }
        public ActionResult Details(long? DriverId)
        {
            if (DriverId.HasValue == true)
            {
                var Driver = db.Drivers.Find(DriverId.Value);
                if (Driver != null)
                {
                    return View(Driver);
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult ChangePassword(long? DriverId)
        {
            if (DriverId.HasValue == true)
            {
                var Driver = db.Drivers.Find(DriverId.Value);
                if (Driver != null)
                {
                    ViewBag.Driver = Driver;
                    return View();
                }
            }
            else
            {
                var Driver = db.Drivers.FirstOrDefault(w => w.UserId == CurrentUserId);
                if (Driver != null)
                {
                    ViewBag.Driver = Driver;
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            //if (User.IsInRole("Driver") == false)
            //{
                if (ModelState.ContainsKey("CurrentPassword"))
                {
                    ModelState["CurrentPassword"].Errors.Clear();
                }
            //}
            if (ModelState.IsValid == true)
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var Driver = db.Drivers.Find(model.DriverId);
                if (Driver != null)
                {
                    Driver.User.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    if (User.IsInRole("Driver"))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.Driver = db.Drivers.Find(model.DriverId);
            return View(model);
        }

        public ActionResult Edit(long? DriverId)
        {
            if (DriverId.HasValue == true)
            {
                var Driver = db.Drivers.Find(DriverId.Value);
                if (Driver != null)
                {
                    ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
                    ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
                    return View(EditDriverVM.ToEditDriverVM(Driver));
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditDriverVM model)
        {
            var Validation = ValidateDriverEdition(model, ModelState);
            if (Validation.IsValid == false)
            {
                goto Jump;
            }

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var City = db.Cities.Find(model.CityId);
                    var Driver = db.Drivers.Find(model.DriverId);
                    Driver.VehicleTypeId = model.VehicleTypeId;
                    Driver.VehicleColor = model.VehicleColor;
                    Driver.VehicleNumber = model.VehicleNumber;

                    if (model.PersonalPhoto != null)
                    {
                        MediaControl.Delete(FilePath.Users, Driver.PersonalPhoto);
                        Driver.PersonalPhoto = MediaControl.Upload(FilePath.Users, model.PersonalPhoto);
                    }
                    if (model.VehicleLicensePhoto != null)
                    {
                        MediaControl.Delete(FilePath.Users, Driver.VehicleLicensePhoto);
                        Driver.VehicleLicensePhoto = MediaControl.Upload(FilePath.Users, model.VehicleLicensePhoto);
                    }
                    if (model.LicensePhoto != null)
                    {
                        MediaControl.Delete(FilePath.Users, Driver.LicensePhoto);
                        Driver.LicensePhoto = MediaControl.Upload(FilePath.Users, model.LicensePhoto);
                    }

                    Driver.User.Name = model.Name;
                    Driver.User.Email = model.Email;
                    Driver.User.UserName = model.Email;
                    Driver.User.PhoneNumber = model.PhoneNumber;
                    Driver.User.CityId = model.CityId;
                    Driver.Profit = model.Profit;

                    CRUD<Driver>.Update(Driver);
                    db.SaveChanges();
                    Transaction.Commit();
                    TempData["Success"] = true;
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    goto Jump;
                }
            }

            Jump:
            ViewBag.VehicleTypes = db.VehicleTypes.Where(w => w.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        private ModelStateDictionary ValidateDriverEdition(EditDriverVM model, ModelStateDictionary ModelState)
        {
            if (model.IdentityPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.IdentityPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("IdentityPhoto", "صوره هوية السائق غير صحيحة");
            }

            if (model.LicensePhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.LicensePhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("LicensePhoto", "صوره رخصة السائق غير صحيحة");
            }

            if (model.PersonalPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.PersonalPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("PersonalPhoto", "صوره السائق الشخصية غير صحيحة");
            }

            if (model.VehicleLicensePhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.VehicleLicensePhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("VehicleLicensePhoto", "صوره رخصه المركبة غير صحيحة");
            }

            if (model.VehicleTypeId.HasValue == true)
            {
                var VehicleType = db.VehicleTypes.FirstOrDefault(w => w.IsDeleted == false && w.Id == model.VehicleTypeId);
                if (VehicleType == null)
                {
                    ModelState.AddModelError("VehicleTypeId", "نوع المركبة غير صحيح");
                }
            }

            var Driver = db.Drivers.Find(model.DriverId);
            if (Driver == null)
            {
                ModelState.AddModelError("", "السائق المطلوب غير متاح");
            }
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", "اسم السائق مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (City != null && (City.Country.NameEn.ToLower() == "egypt" || City.Country.NameAr.ToLower() == "مصر"))
                    {
                        string EgyptPhoneNumberSyntax = @"^01[0-3|5]{1}[0-9]{8}";
                        bool IsPhoneNumber = Regex.IsMatch(model.PhoneNumber, EgyptPhoneNumberSyntax, RegexOptions.IgnoreCase);
                        if (IsPhoneNumber == true)
                        {
                            if (model.PhoneNumber.Length != 11)
                            {
                                ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                        }
                    }
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                    else
                    {
                        if (UserValidation.IsPhoneExists(model.PhoneNumber, Driver.UserId) == true)
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (UserValidation.IsEmailExists(model.Email, Driver.UserId) == true)
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");
                }
            }

            return ModelState;
        }

        public ActionResult Control(long? DriverId)
        {
            if (DriverId.HasValue == true)
            {
                var Driver = db.Drivers.Find(DriverId.Value);
                if (Driver != null)
                {
                    return View(Driver);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> ToggleStop(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Driver = db.Drivers.Find(Id.Value);
            if (Driver == null || Driver.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Driver.IsBlocked == true)
            {
                Driver.IsBlocked = false;
                CRUD<Driver>.Update(Driver);
                await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
            }
            else
            {
                Driver.IsBlocked = true;
                CRUD<Driver>.Update(Driver);
                await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { DriverId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> ToggleDelete(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Driver = db.Drivers.Find(Id.Value);
            if (Driver == null || Driver.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Driver.IsDeleted == true)
            {
                await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                CRUD<Driver>.Restore(Driver);
            }
            else
            {
                await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                CRUD<Driver>.Delete(Driver);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { DriverId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> Accept(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Driver = db.Drivers.Find(Id.Value);
            if (Driver == null)
                return RedirectToAction("Index");

            Driver.IsAccepted = true;
            CRUD<Driver>.Update(Driver);
            db.SaveChanges();
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم قبولك", "تهانينا ، لقد تم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم قبولك", "تهانينا ، لقد تم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            return RedirectToAction("Control", new { DriverId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> Reject(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Driver = db.Drivers.Find(Id.Value);
            if (Driver == null)
                return RedirectToAction("Index");

            Driver.IsAccepted = false;
            CRUD<Driver>.Update(Driver);
            db.SaveChanges();
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "لم يتم قبولك", "لم يتم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "لم يتم قبولك", "لم يتم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            return RedirectToAction("Control", new { DriverId = Id });
        }
    }
}