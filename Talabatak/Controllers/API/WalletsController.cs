using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Talabatak.Models.Domains;
using System.Threading.Tasks;
using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using Talabatak.SignalR;

namespace Talabatak.Controllers.API
{
    [RoutePrefix("api/wallets")]
    [AllowAnonymous]
    public class WalletsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public WalletsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("WalletData")]
        public IHttpActionResult GetWalletData(string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find(CurrentUserId);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.Unauthorized, baseResponse);
            }

            decimal WalletMinimumAmountToCharge = 0;
            decimal WalletMaximumAmountToCharge = 0;
            var MetaData = db.MetaDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (MetaData != null)
            {
                WalletMinimumAmountToCharge = MetaData.WalletMinimumAmountToCharge;
                WalletMaximumAmountToCharge = MetaData.WalletMaximumAmountToCharge;
            }

            baseResponse.Data = new
            {
                WalletMinimumAmountToCharge,
                WalletMinimumAmountToChargeText = WalletMinimumAmountToCharge + (string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " جنيه" : " LE"),
                WalletMaximumAmountToCharge,
                WalletMaximumAmountToChargeText = WalletMaximumAmountToCharge + (string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " جنيه" : " LE"),
                UserWallet = user.Wallet,
                UserWalletText = user.Wallet + (string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " جنيه" : " LE")
            };
            return Ok(baseResponse);
        }
        [HttpGet]
        [Route("Charge")]
        public async Task<IHttpActionResult> ChargeWallet(decimal Amount)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find("1685335831654608");
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.Unauthorized, baseResponse);
            }

            var MetaData = db.MetaDatas.FirstOrDefault(w => w.IsDeleted == false);

            if (MetaData != null)
            {
                if (Amount < MetaData.WalletMinimumAmountToCharge)
                {
                    baseResponse.ErrorCode = Errors.AmountIsLowerThanMinimumAmountToBeCharged;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
                if (Amount > MetaData.WalletMaximumAmountToCharge)
                {
                    baseResponse.ErrorCode = Errors.AmountIsHigherThanMaximumAmountToBeCharged;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }

            user.PaymentUniqueKey = RandomGenerator.GenerateString(7);
            user.AmountToBePaid = Amount;
            db.SaveChanges();
            var PaymentLink = await PaymentGateway.GenerateChargeWalletPaymentLink(user);
            if (PaymentLink == null)
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
            baseResponse.Data = new
            {
                RedirectUrl = PaymentLink,
                user.PaymentUniqueKey,
                Amount
            };
            return Ok(baseResponse);
        }
        [HttpGet]
        [Route("Cancel")]
        public IHttpActionResult Cancel(PaymentStatusDto model)
        {
            return Ok("http://localhost:44361/Home/Cancel");
        }
        [HttpGet]
        [Route("Success")]
        public IHttpActionResult Success(PaymentStatusDto model)
        {
            return Ok("http://localhost:44361/Home/Sucess");
        }
        [HttpPost]
        [Route("Notify")]
        public async Task<IHttpActionResult> Notify(AcceptStatusDto model)
        {
            
            if (!bool.Parse(model.obj["success"]))
            {
                baseResponse.ErrorCode = Errors.PaymentMethodIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
           /* var result = await PaymentGateway.PaymentStatus(model);*/          
                if(db.Users.Any(x => x.PaymentUniqueKey == model.order["id"]))
                {
                    var user = db.Users.FirstOrDefault(x => x.PaymentUniqueKey == model.order["id"]);
                    user.Wallet +=  model.amount_cents;
                    db.SaveChanges();
                }
                if (db.StoreOrders.Any(x => x.Id ==long.Parse(model.order["id"]) && x.Total == model.amount_cents && !x.IsDeleted))
            {
                var Order = db.StoreOrders.Include(i => i.Items).FirstOrDefault(w => w.Id == long.Parse(model.order["id"]) && w.IsDeleted == false);
                ApplicationUser Admin = db.Users.FirstOrDefault(d => d.Id == "6cbac676-8e4a-4e01-ace4-43814d464519");
                Admin.Wallet += (Order.DeliveryFees * ((decimal)(Order.Driver.Profit) / (decimal)100.0));
                db.SaveChanges();
                var items = db.StoreOrderItems.Include(d => d.Product).Include(d => d.Product.Category)
                        .Where(d => d.OrderId == Order.Id).ToList();
                Store store;
                foreach (var item in items)
                {
                    new Store();
                    store = await db.Stores.FirstOrDefaultAsync(d => d.Id == item.Product.Category.StoreId);
                    store.Wallet += (item.SubTotal - ((decimal)item.SubTotal * ((decimal)store.Profit / (decimal)100.0)));
                    db.SaveChanges();
                    Admin.Wallet += ((decimal)item.SubTotal * ((decimal)store.Profit / (decimal)100.0));
                    db.SaveChanges();
                }
                Order.Status = StoreOrderStatus.Preparing;
                Order.IsPaid = true;
                Order.DeliveredOn = DateTime.Now.ToUniversalTime();
                db.SaveChanges();
                Order.Driver.User.Wallet += (Order.DeliveryFees - (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0)));
                db.SaveChanges();
                db.UserWallets.Add(new UserWallet()
                {
                    StoreOrderId = long.Parse(model.order["id"]),
                    TransactionAmount = Order.Total,
                    TransactionType = TransactionType.UserPaidForStoreOrderByWallet,
                    UserId = Order.UserId
                });
                db.SaveChanges();
                db.UserWallets.Add(new UserWallet()
                {
                    StoreOrderId = long.Parse(model.order["id"]),
                    TransactionType = TransactionType.AddedByAdminManually,
                    UserId = Order.Driver.UserId,
                    TransactionAmount = (Order.DeliveryFees - (Order.DeliveryFees * ((decimal)(Order.Driver.Profit) / (decimal)100.0)))
                });
                db.SaveChanges();
                var Item = Order.Items.FirstOrDefault(s => s.IsDeleted == false);
                Store Store = null;
                if (Item != null)
                {
                    Store = Item.Product.Category.Store;
                    Order.StoreProfit = Store.Profit;
                }
                db.SaveChanges();
                if (Store.DeliveryBySystem)
                {
                    var Drivers = StoreOrderActions.GetAvailableDrivers(Store, Order.Id);
                    if (Drivers.Count > 0)
                    {
                        foreach (var driver in Drivers)
                        {
                            if ((db.Users.FirstOrDefault(w => w.Id == driver.UserId).Wallet) > 0)
                            {
                                await StoreOrderActions.AssignOrderToDriverAsync(Order, driver);
                            }
                        }
                    }
                    var x = db.SaveChanges() > 0;
                }
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                OrdersHub.Clients.Group("StoreOrdersDashboard").add(Order.Id);
                string Title = $"تم استقبال طلبكم";
                string Body = $"جارى الان تجهيز الطلب واعطائه لاقرب سائق متوفر";
                await SMS.SendMessage("", "01024401763", "هناك طلب جديد من فضلك قم بزيارة لوحة التحكم");
                Notifications.SendWebNotification("تم أستقبال طلب جديد", Body, "5ad497d6-1abb-455b-bdcf-3b08ade12f79", -1, "Amdin", Order.Id, NotificationType.WebStoreOrderDetailsPage, true);
                await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                baseResponse.Data = Order.PaymentMethod;
                return Ok(baseResponse);
            }
            baseResponse.Data = "";
            return Ok(baseResponse);
        }
    }
}