using Talabatak.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Models.ViewModels;
using Talabatak.SignalR;
using Talabatak.Models.Enums;

namespace Talabatak.Controllers.MVC
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult IndexAr()
        {
            return View();
        }
        public ActionResult Report(DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            HomeReportVM model = new HomeReportVM();
                model.DriverAvailable = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsAvailable == true && x.IsDeleted == false
               && x.IsBlocked == false).ToList().Count;
            
                model.DriverDeleted = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)  && x.IsDeleted == false).ToList().Count;
            
                model.DriverBlocked = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsBlocked == true ).ToList().Count;
          
                model.WorkerAvailable = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsAvailable == true && x.IsDeleted == false
               && x.IsBlocked == false).ToList().Count;
           
                model.WorkerDeleted = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsDeleted == false).ToList().Count;

          
                model.WorkerBlocked = db.Drivers.Where(x => (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.IsBlocked == true).ToList().Count;


            model.AmdinWallet = db.UserWallets.Where(x => x.TransactionType == TransactionType.AddedByAdminManually).ToList().Sum(x => x.TransactionAmount);
            model.DoneWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.Finished).ToList().Count;

            model.CancelWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.AcceptedByWorker).ToList().Count
                + db.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByAdmin).ToList().Count; ;

            model.RejectedWorkerOrder = db.JobOrders.Where(x => x.Status == JobOrderStatus.RejectedByWorker).ToList().Count;

            model.DoneDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).ToList().Count+
                 db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList().Count;

            model.CancelDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Cancelled).ToList().Count +
     db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Cancelled).ToList().Count;

            model.RejectedDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Rejected).ToList().Count;

            model.DoneDriverOrder = db.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).ToList().Count +
     db.OtlobAy7agaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList().Count;
            return View(model);
        }
        public ActionResult Sucess()
        {
            return View();
        }
        public ActionResult Cancel()
        {
            return View();
        }
        public ActionResult SetCulture(string culture, string ReturnUrl)
        {
            if (culture == "en-us" || culture == "ar")
            {
                HttpCookie cookie = Request.Cookies["_Talabatakculture"];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddYears(-100);
                    cookie = new HttpCookie("_Talabatakculture");
                    cookie.Value = culture;
                }
                else
                {
                    cookie = new HttpCookie("_Talabatakculture");
                    cookie.Value = culture;
                    cookie.Expires = DateTime.Now.AddYears(1);
                }
                Response.Cookies.Add(cookie);
            }
            return Redirect(ReturnUrl);
        }
    }
}
