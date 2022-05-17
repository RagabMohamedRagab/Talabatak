using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Talabatak.Models.ViewModels;
namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class OtlobAy7agaController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var Orders = db.OtlobAy7agaOrders.Where(s => s.IsDeleted == false && s.OrderStatus != OtlobAy7agaOrderStatus.Initialized).OrderByDescending(q => q.CreatedOn).ToList();
            return View(Orders);
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChangeDriver(int? OrderId , string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue)
            {
                ViewBag.Drivers = new SelectList(db.Drivers.Include(x => x.User).ToList(), "Id", "User.Name");
                ChangeDriver model = new ChangeDriver()
                {
                    OrderId = Convert.ToInt32(OrderId)
                };
                return View(model);
            }
            return Redirect(ReturnUrl);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeDriver(ChangeDriver model)
        {
            if (model.DriverId != 0)
            {
                var order = await db.OtlobAy7agaOrders.FirstOrDefaultAsync(x => x.Id == model.OrderId);
                order.DriverId = null;
                order.OrderStatus = OtlobAy7agaOrderStatus.Placed;
                var driver = db.Drivers.FirstOrDefault(x => x.Id == model.DriverId);
                await OtlobAy7agaOrderActions.SendOrderToDriver(model.OrderId, driver);
                if (await db.SaveChangesAsync() > 0)
                {}
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Details(long? OrderId,string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue == true)
            {
                var Order = db.OtlobAy7agaOrders.FirstOrDefault(w => w.IsDeleted == false && w.OrderStatus != OtlobAy7agaOrderStatus.Initialized && w.Id == OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return Redirect(ReturnUrl);
        }
        public async Task<ActionResult> Finshed(long? OrderId)
        {
            if (await db.OtlobAy7agaOrders.AnyAsync(x => x.Id == OrderId))
            {
                var FinshedOrder = await db.OtlobAy7agaOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                FinshedOrder.OrderStatus = OtlobAy7agaOrderStatus.Finished;
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<ActionResult> OrderCancel(long? OrderId)
        {
            if (await db.OtlobAy7agaOrders.AnyAsync(x => x.Id == OrderId))
            {
                var canceldOrder = await db.OtlobAy7agaOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                canceldOrder.OrderStatus = OtlobAy7agaOrderStatus.Cancelled;
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Chat(long? OrderId, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue == true)
            {
                var Order = db.OtlobAy7agaOrders.FirstOrDefault(w => w.IsDeleted == false && w.OrderStatus != OtlobAy7agaOrderStatus.Initialized && w.Id == OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return Redirect(ReturnUrl);
        }
    }
}