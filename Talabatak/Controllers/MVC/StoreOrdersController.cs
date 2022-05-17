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
using ClosedXML.Excel;
using System.IO;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin, Store")]
    public class StoreOrdersController : BaseController
    {
        public ActionResult Index(long? StoreId=1)
        {
            
            if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
            {
                var Orders = db.StoreOrders.Include(d=>d.Items).Where(s => s.IsDeleted == false && s.Status != StoreOrderStatus.Initialized).OrderByDescending(s => s.CreatedOn).ToList();
                if (StoreId.HasValue == true)
                {
                    Orders = Orders.Where(w => w.Items.Any(s => s.Product.Category.StoreId == StoreId)).ToList();
                    ViewBag.Store = db.Stores.Find(StoreId);
                }
                TempData["order"] = Orders;
                ViewBag.Stores = db.Stores.Where(s => s.IsDeleted == false).ToList();
                return View(Orders);
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
                        var Orders = db.StoreOrders.Where(w => w.IsDeleted == false && w.Status != StoreOrderStatus.Initialized).OrderByDescending(w => w.CreatedOn).ToList();
                        Orders = Orders.Where(w => w.Items.Any(s => s.Product.Category.StoreId == StoreId)).ToList();
                        TempData["order"] = Orders;
                        return View(Orders);
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
        public async Task<ActionResult> OrderCancel(long? OrderId)
        {
            if (await db.StoreOrders.AnyAsync(x => x.Id == OrderId))
            {
                var canceldOrder = await db.StoreOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                if (canceldOrder.PaymentMethod == PaymentMethod.Online && canceldOrder.IsPaid)
                {
                    canceldOrder.User.Wallet += canceldOrder.Total;
                    db.UserWallets.Add(new UserWallet()
                    {
                        StoreOrderId = OrderId,
                        TransactionAmount = canceldOrder.Total,
                        TransactionType = TransactionType.RefundTheUser,
                        UserId = canceldOrder.UserId
                    });
                    db.SaveChanges();
                }
                canceldOrder.Status = StoreOrderStatus.Cancelled;
                canceldOrder.IsPaid = false;
                await db.SaveChangesAsync();

            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<ActionResult> Finshed(long? OrderId)
        {
            if (await db.StoreOrders.AnyAsync(x => x.Id == OrderId))
            {
                var FinshedOrder = await db.StoreOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                FinshedOrder.Status = StoreOrderStatus.Finished;
                FinshedOrder.IsPaid = true;
                await db.SaveChangesAsync();
                EmailSender sender = new EmailSender();
                try
                {
                    sender.PrepareHTMLandSend(FinshedOrder, "en");
                }
                catch (Exception) { }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChangeDriver(int? OrderId, string ReturnUrl)
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
                var order = await db.StoreOrders.FirstOrDefaultAsync(x => x.Id == model.OrderId);
                order.DriverId = null;
                order.Status = StoreOrderStatus.Preparing;
                if (await db.SaveChangesAsync() > 0)
                { }
                var driver = db.Drivers.FirstOrDefault(x => x.Id == model.DriverId);
                await StoreOrderActions.AssignOrderToDriverAsync(order, driver);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<ActionResult> Accept(long? OrderId, string ReturnUrl)
        {
            bool GoToAcceptance = false;
            if (OrderId.HasValue == true)
            {
                var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && w.Status == StoreOrderStatus.Placed && w.Id == OrderId);
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    if (Order != null)
                    {
                        GoToAcceptance = true;
                    }
                }
                if (User.IsInRole("Store"))
                {
                    if (Order != null)
                    {
                        var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                        if (Item != null)
                        {
                            if (Item.Product.Category.Store.Owners.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId))
                            {
                                GoToAcceptance = true;
                            }
                        }
                    }
                }
                if (GoToAcceptance == true)
                {
                    Order.Status = StoreOrderStatus.Preparing;
                    var Item = Order.Items.FirstOrDefault(s => s.IsDeleted == false);
                    Store Store = null;
                    if (Item != null)
                    {
                        Store = Item.Product.Category.Store;
                    }
                    var Drivers = StoreOrderActions.GetAvailableDrivers(Store, Order.Id);
                    if (Drivers != null)
                    {
                        foreach (var driver in Drivers)
                        {
                            await StoreOrderActions.AssignOrderToDriverAsync(Order, driver);
                        }
                    }
                }
                db.SaveChanges();
            }
            return Redirect(ReturnUrl);
        }

        public async Task<ActionResult> Reject(long? OrderId, string ReturnUrl)
        {
            bool GoToRejection = false;
            if (OrderId.HasValue == true)
            {
                var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && (w.Status == StoreOrderStatus.Placed || w.Status == StoreOrderStatus.Preparing) && w.Id == OrderId);
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    if (Order != null)
                    {
                        GoToRejection = true;
                    }
                }
                if (User.IsInRole("Store"))
                {
                    if (Order != null)
                    {
                        var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                        if (Item != null)
                        {
                            if (Item.Product.Category.Store.Owners.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId))
                            {
                                GoToRejection = true;
                            }
                        }
                    }
                }
                if (GoToRejection == true)
                {
                    Order.Status = StoreOrderStatus.Rejected;
                    if (Order.DriverId.HasValue == true)
                    {
                        await Notifications.SendToAllSpecificAndroidUserDevices(Order.Driver.UserId, $"تم الغاء الطلب رقم {Order.Code}", $"عذراً ، تم الغاء الطلب رقم {Order.Code}", NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
                        await Notifications.SendToAllSpecificIOSUserDevices(Order.Driver.UserId, $"تم الغاء الطلب رقم {Order.Code}", $"عذراً ، تم الغاء الطلب رقم {Order.Code}", NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
                    }
                    await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, $"تم رفض الطلب رقم {Order.Code}", $"عذراً ، تم رفض طلبكم رقم {Order.Code}", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, $"تم رفض الطلب رقم {Order.Code}", $"عذراً ، تم رفض طلبكم رقم {Order.Code}", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                }
                db.SaveChanges();
            }
            return Redirect(ReturnUrl);
        }

        public ActionResult Details(long? OrderId, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue == true)
            {
                var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && w.Status != StoreOrderStatus.Initialized && w.Id == OrderId);
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    if (Order != null)
                    {
                        return View(Order);
                    }
                }
                if (User.IsInRole("Store"))
                {
                    if (Order != null)
                    {
                        var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                        if (Item != null)
                        {
                            if (Item.Product.Category.Store.Owners.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId))
                            {
                                return View(Order);
                            }
                        }
                    }
                }
            }
            return Redirect(ReturnUrl);
        }

        public ActionResult Chat(long? OrderId, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue == true)
            {
                var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && w.Status != StoreOrderStatus.Initialized && w.Id == OrderId);
                if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                {
                    if (Order != null)
                    {
                        return View(Order);
                    }
                }
                if (User.IsInRole("Store"))
                {
                    if (Order != null)
                    {
                        var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                        if (Item != null)
                        {
                            if (Item.Product.Category.Store.Owners.Any(s => s.IsDeleted == false && s.UserId == CurrentUserId))
                            {
                                return View(Order);
                            }
                        }
                    }
                }
            }
            return Redirect(ReturnUrl);
        }
        public async Task<ActionResult> DownloadExecl()
        {
            var order = TempData["order"] as List<StoreOrder>;
            var dt = ExcelExport.OrdersExport(order);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
                }
            }
        }
        [HttpGet]
        public ActionResult RenderOrderData(long OrderId)
        {
            var Order = db.StoreOrders.Find(OrderId);
            if (Order != null)
            {
                return PartialView(Order);
            }
            else
            {
                return null;
            }
        }
    }
}