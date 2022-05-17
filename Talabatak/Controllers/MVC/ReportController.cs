using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.Models.ViewModels;

namespace Talabatak.Controllers.MVC
{
    /*[AllowAnonymous]*/
    public class ReportController : BaseController
    {
        /*protected ApplicationDbContext db = System.Web.HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
*/
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Driver(long? DriverId, DateTime? FilterTime, DateTime? FilterTimeTo)
        {

        
            DriverReportVM model = new DriverReportVM();
            if (!DriverId.HasValue)
            {
                return View(nameof(Index));
            }
            if (DriverId.HasValue == true)
            {
                if (!await db.Drivers.AnyAsync(x => x.Id == DriverId.Value))
                {
                    return View(nameof(Index));
                }
            }
            model.Total = 0;
            model.Driver = await db.Drivers.Include(x => x.User).Include(x => x.User.City).Include(x => x.User.City.Country).FirstOrDefaultAsync(x => x.Id == DriverId.Value);

            if (db.OtlobAy7agaOrders.Any(x => x.DriverId == DriverId && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Total = db.OtlobAy7agaOrders.Where(x => x.DriverId == DriverId && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
               (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.DeliveryFees);
            }
            if (db.StoreOrders.Any(x => x.DriverId == DriverId && x.Status == StoreOrderStatus.Finished
            && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Total += db.StoreOrders.Where(x => x.DriverId == DriverId && x.Status == StoreOrderStatus.Finished
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
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
            model.OtlobAy7AgaOrders = new List<OtlobAy7agaOrder>();
            if (db.StoreOrders.Any(x => x.DriverId == DriverId
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.StoreOrders = await db.StoreOrders.Where(x => x.DriverId == DriverId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            if (db.OtlobAy7agaOrders.Any(x => x.DriverId == DriverId
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.OtlobAy7AgaOrders = await db.OtlobAy7agaOrders.Where(x => x.DriverId == DriverId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            foreach (var Order in db.StoreOrders.Where(x => (x.DriverId == DriverId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == StoreOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.OtlobAy7agaOrders.Where(x => (x.DriverId == DriverId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            model.Stil = model.ForSystem - model.Paid;
            ViewBag.Title = $"تقرير السائق {model.Driver.User.Name}";
            model.Done = (model.OtlobAy7AgaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Finished).Count()) +
                        (model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Finished).Count());
            model.Cancel = (model.OtlobAy7AgaOrders.Where(x => x.OrderStatus == OtlobAy7agaOrderStatus.Cancelled).Count()) +
            (model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Cancelled).Count());

            model.Reject = model.StoreOrders.Where(x => x.Status == StoreOrderStatus.Rejected).Count();
            model.driverReviews = db.DriverReviews.Include(x => x.User).Where(x => x.DriverId == model.Driver.Id).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Worker(long? WorkerId, DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            DriverReportVM model = new DriverReportVM();
            if (!WorkerId.HasValue)
            {
                return View(nameof(Index));
            }
            if (WorkerId.HasValue == true)
            {
                if (!await db.Workers.AnyAsync(x => x.Id == WorkerId.Value))
                {
                    return View(nameof(Index));
                }
            }
            model.Total = 0;
            model.Worker = await db.Workers.Include(x => x.User).Include(x => x.User.City).Include(x => x.User.City.Country).FirstOrDefaultAsync(x => x.Id == WorkerId.Value);

            if (db.JobOrders.Any(x => x.WorkerId == WorkerId && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))

            {
                model.Total = db.JobOrders.Where(x => x.WorkerId == WorkerId && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TotalPrice);
            }

            model.Paid = 0;
            if (db.UserWallets.Any(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid += db.UserWallets.Where(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            else if (db.UserWallets.Any(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                      && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                        (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid -= db.UserWallets.Where(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }

            model.JobOrders = new List<JobOrder>();
            if (db.JobOrders.Any(x => x.WorkerId == WorkerId
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.JobOrders = await db.JobOrders.Where(x => x.WorkerId == WorkerId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.JobOrders.Where(x => (x.WorkerId == WorkerId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == JobOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.TotalPrice * ((decimal)Order.WorkerProfit / (decimal)100.0);
            }
            model.Stil = model.ForSystem - model.Paid;
            model.Done = model.JobOrders.Where(x => x.Status == JobOrderStatus.Finished).Count();
            model.Cancel = (model.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByUser).Count())
                + (model.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByAdmin).Count());
            model.Reject = model.JobOrders.Where(x => x.Status == JobOrderStatus.RejectedByWorker).Count();
            ViewBag.Title = $"تقرير العامــل {model.Worker.User.Name}";
            return View(model);
        }
    }
}