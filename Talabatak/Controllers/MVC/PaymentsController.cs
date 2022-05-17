using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Microsoft.AspNet.SignalR;

namespace Talabatak.Controllers.MVC
{
    [AllowAnonymous]
    public class PaymentsController  : BaseController
    {
        [AllowAnonymous]
        public ActionResult StoreOrderPayment(string Order)
        {
            var Cart = db.StoreOrders.FirstOrDefault(w => w.PaymentUniqueKey == Order);
            if (Cart == null || string.IsNullOrEmpty(Order))
            {
                return null;
            }
            return View(Cart);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> StoreOrderSuccess(string Order)
        {
            if (string.IsNullOrEmpty(Order))
                return null;

            var Params = Order.Split('?');
            string Code = null;
            string TransactionId = null;
            if (Params != null && Params.Count() == 2)
            {
                Code = Params.ElementAtOrDefault(0);
                TransactionId = Params.ElementAtOrDefault(1).Substring(14);
            }
            if (Code == null || TransactionId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var UserOrder = db.StoreOrders.FirstOrDefault(w => w.PaymentUniqueKey == Code && w.IsDeleted == false && w.Status == StoreOrderStatus.Initialized);
            if (UserOrder != null)
            {
                db.PaymentHistories.Add(new PaymentHistory()
                {
                    IsStoreOrder = true,
                    StoreOrderId = UserOrder.Id,
                    TransactionId = TransactionId
                });
                UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
                UserOrder.Status = StoreOrderStatus.Placed;
                UserOrder.PaymentMethod = PaymentMethod.Online;
                UserOrder.IsPaid = true;
                CRUD<StoreOrder>.Update(UserOrder);
                db.SaveChanges();
                // ****************************
                // to be removed later and activate this code in StoreOrdersController Accept Action
                UserOrder.Status = StoreOrderStatus.Preparing;
                var Item = UserOrder.Items.FirstOrDefault(s => s.IsDeleted == false);
                Store Store = null;
                if (Item != null)
                {
                    Store = Item.Product.Category.Store;
                }
                var Drivers = StoreOrderActions.GetAvailableDrivers(Store, UserOrder.Id);
                if (Drivers != null)
                {
                    foreach (var driver in Drivers)
                    {
                        await StoreOrderActions.AssignOrderToDriverAsync(UserOrder, driver);
                    }
                }
                db.SaveChanges();
                // ****************************
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<SignalR.OrdersHub>();
                OrdersHub.Clients.Group("StoreOrdersDashboard").add(UserOrder.Id);
                string Title = $"تم استقبال طلبكم";
                string Body = $"جارى الان تجهيز الطلب واعطائه لاقرب سائق متوفر";
                await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, UserOrder.Id, false, true);
                await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, UserOrder.Id, false, true);
                return View();
            }
            TempData["Failed"] = true;
            return RedirectToAction("StoreOrderFailed");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult StoreOrderFailed()
        {
            if (TempData["Failed"] != null)
            {
                return View();
            }
            return null;
        }

        [AllowAnonymous]
        public ActionResult ChargeWallet(string PayId)
        {
            var user = db.Users.FirstOrDefault(w => w.PaymentUniqueKey == PayId);
            if (user == null || string.IsNullOrEmpty(PayId))
            {
                return null;
            }
            return View(user);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ChargeWalletSuccess(string Order)
        {
            if (string.IsNullOrEmpty(Order))
                return null;

            var Params = Order.Split('?');
            string Code = null;
            string TransactionId = null;
            if (Params != null && Params.Count() == 2)
            {
                Code = Params.ElementAtOrDefault(0);
                TransactionId = Params.ElementAtOrDefault(1).Substring(14);
            }
            if (Code == null || TransactionId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var user = db.Users.FirstOrDefault(w => w.PaymentUniqueKey == Code);
            if (user != null)
            {
                db.UserWallets.Add(new UserWallet()
                {
                    TransactionId = TransactionId,
                    TransactionAmount = user.AmountToBePaid,
                    TransactionType = TransactionType.UserChargedTheWallet,
                    UserId = user.Id
                });
                user.Wallet += user.AmountToBePaid;
                db.SaveChanges();
                string Title = $"تم شحن المحفظه بنجاح";
                string Body = $"تم شحن محفظتكم بمبلغ [{user.Wallet}] جنيه";
                await Notifications.SendToAllSpecificAndroidUserDevices(user.Id, Title, Body, NotificationType.ApplicationWalletPage, -1, false, true);
                await Notifications.SendToAllSpecificIOSUserDevices(user.Id, Title, Body, NotificationType.ApplicationWalletPage, -1, false, true);
                user.AmountToBePaid = 0;
                user.PaymentUniqueKey = null;
                db.SaveChanges();
                return View();
            }
            TempData["Failed"] = true;
            return RedirectToAction("ChargeWalletFailed");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ChargeWalletFailed()
        {
            if (TempData["Failed"] != null)
            {
                return View();
            }
            return null;
        }
    }
}