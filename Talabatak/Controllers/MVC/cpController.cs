using Talabatak.Models.Domains;
using Talabatak.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Models.Enums;
using System.Data.Entity;

namespace Talabatak.Controllers.MVC
{
    public class cpController : BaseController
    {
        [Authorize(Roles = "Admin, SubAdmin")]
        public ActionResult Index(DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            HomeReportVM model = new HomeReportVM();
            model.DriverAvailable = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsAvailable == true && x.IsDeleted == false
           && x.IsBlocked == false).ToList().Count;

            model.DriverDeleted = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsDeleted == false).ToList().Count;

            model.DriverBlocked = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsBlocked == true).ToList().Count;

            model.WorkerAvailable = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsAvailable == true && x.IsDeleted == false
           && x.IsBlocked == false).ToList().Count;

            model.WorkerDeleted = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsDeleted == false).ToList().Count;


            model.WorkerBlocked = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsBlocked == true).ToList().Count;

            model.StoreAvilable = db.Stores.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsOpen == true).ToList().Count();
            
            model.StoreDeleted = db.Stores.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsDeleted == true).ToList().Count();

                 model.StoreBlocked = db.Stores.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsBlocked == true).ToList().Count();


           
            foreach(var Order in db.StoreOrders.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
            &&(FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == StoreOrderStatus.Finished).ToList())
            {
                model.DriverStoreIncoming += Order.DeliveryFees *((decimal) Order.DeliveryProfit / (decimal)100.0) ;
            }
            foreach (var Order in db.OtlobAy7agaOrders.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.OrderStatus== OtlobAy7agaOrderStatus.Finished).ToList())
            {
                model.DriverOtlobIncoming += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            foreach (var Order in db.JobOrders.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == JobOrderStatus.Finished).ToList())
            {
                model.WorkerIncoming += Order.TotalPrice * ((decimal)Order.WorkerProfit / (decimal)100.0);
            }
            foreach (var Order in db.StoreOrders.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == StoreOrderStatus.Finished).ToList())
            {
                model.StoreIncoming += Order.SubTotal * ((decimal)Order.StoreProfit / (decimal)100.0);
            }
            model.DoneWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.Finished).ToList().Count;

            model.CancelWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.AcceptedByWorker).ToList().Count
                + db.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByAdmin).ToList().Count; ;

            model.RejectedWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.RejectedByWorker).ToList().Count;

            model.DoneDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).ToList().Count +
                 db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList().Count;

            model.CancelDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Cancelled).ToList().Count +
            db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Cancelled).ToList().Count;

            model.RejectedDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Rejected).ToList().Count;

            model.DoneDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).ToList().Count +
            db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList().Count;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (UserManager.IsInRole(CurrentUserId, "Admin"))
                    return RedirectToAction("Index", "cp");

                if (UserManager.IsInRole(CurrentUserId, "Store"))
                {
                    var StoreId = db.StoreUsers.FirstOrDefault(x => x.UserId == CurrentUserId&& (!x.IsDeleted)).StoreId;
                    if (StoreId != 0)
                    {
                        return RedirectToAction("Report", "Stores", new { id = StoreId });
                    }
                }
                return RedirectToAction("index", "cp");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(cpLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    CurrentUserId = UserManager.FindByEmail(model.Email).Id;
                    if (UserManager.IsInRole(CurrentUserId, "Admin"))
                        return RedirectToAction("index", "cp");

                    if (UserManager.IsInRole(CurrentUserId, "Store"))
                    {
                        var Store = db.StoreUsers.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId);
                        if (Store == true)  
                        {
                            var StoreId = db.StoreUsers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId).Id;
                            return RedirectToAction("Report", "Stores", new { id = StoreId });
                        }                         
                        else
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            ModelState.AddModelError("", "ليس لديك الصلاحيات للدخول");
                            return View(model);
                        }
                    }

                    if (UserManager.IsInRole(CurrentUserId, "SubAdmin"))
                    {
                        var SubAdmin = db.Users.Any(s => s.IsDeleted == false && s.Id == CurrentUserId);
                        if (SubAdmin == true)
                            return RedirectToAction("index", "cp");
                        else
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            ModelState.AddModelError("", "ليس لديك الصلاحيات للدخول");
                            return View(model);
                        }
                    }
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Index", "Home");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "محاولة دخول خاطئه");
                    return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.IsValid == true)
            {
                var user = db.Users.Find(CurrentUserId);
                if (UserManager.CheckPassword(user, model.CurrentPassword) == false) 
                {
                    ModelState.AddModelError("CurrentPassword", "كلمه السر الحالية غير صحيحة");
                    return View(model);
                }
                if (user != null)
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

    }
}