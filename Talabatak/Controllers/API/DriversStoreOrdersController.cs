using Talabatak.Filters;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Talabatak.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace Talabatak.Controllers.API
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/driversStoreOrders")]
    public class DriversStoreOrdersController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private string CurrentUserId;
        private Driver Driver;

        public DriversStoreOrdersController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("Accept")]
        public async Task<IHttpActionResult> AcceptOrder(long OrderId)
        {
            CurrentUserId = User.Identity.GetUserId();
            Driver = db.Drivers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.StoreOrderDrivers.FirstOrDefault(w => w.IsDeleted == false && w.DriverId == Driver.Id && w.OrderId == OrderId && w.IsAccepted.HasValue == false);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }


            if (Order.Order.Status != StoreOrderStatus.Preparing)
            {
                baseResponse.ErrorCode = Errors.CannotAcceptOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Order.Order.DriverId.HasValue == true)
            {
                baseResponse.ErrorCode = Errors.OrderAcceptedByAnotherDriver;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var MetaData = db.MetaDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (MetaData != null && DriversActions.GetDriverOrdersCount(Driver) >= MetaData.NumberOfAvailableOrdersPerDriver)
            {
                baseResponse.ErrorCode = Errors.DriverReachedMaximumNumberOfAllowedOrders;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            Order.IsAccepted = true;
            Order.AcceptedOn = DateTime.Now.ToUniversalTime();
            Order.Order.DriverId = Driver.Id;
            Order.Order.Status = StoreOrderStatus.Delivering;
            StoreOrder store = db.StoreOrders.FirstOrDefault(x => x.Id == Order.OrderId);
            store.DeliveryProfit = Driver.Profit;
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            var AnotherDrivers = db.StoreOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == OrderId && s.DriverId != Driver.Id).ToList();
            var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
            OrdersHub.Clients.Group("StoreOrdersDashboard").update(Order.OrderId);
            foreach (var driver in AnotherDrivers)
            {
                OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // to remove the order from his page
            }
            string Title = $"تم قبول الطلب رقم {Order.Order.Code}";
            string Body = $"برجاء التوجه الى المتجر لاستلام الطلب والبدأ فى التوصيل";
            Notifications.SendWebNotification(Title, "السائق متجهه الأن لأستلام الطلب والبداء فى التوصيل", "6cbac676-8e4a-4e01-ace4-43814d464519", -1,"Amdin", Order.OrderId, NotificationType.WebStoreOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Reject")]
        public async Task<IHttpActionResult> RejectOrder(long OrderId)
        {
            CurrentUserId = User.Identity.GetUserId();
            Driver = db.Drivers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.StoreOrderDrivers.FirstOrDefault(w => w.IsDeleted == false && w.DriverId == Driver.Id && w.OrderId == OrderId && w.IsAccepted.HasValue == false);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.IsAccepted = false;
            Order.RejectedOn = DateTime.Now.ToUniversalTime();
            db.SaveChanges();
            string Title = $"تم رفض الطلب رقم {Order.Order.Code}";
            string Body = $"لقد قمت برفض الطلب رقم {Order.Order.Code}";
            Notifications.SendWebNotification(Title, "قام السائق برفض الطلب", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.OrderId, NotificationType.WebStoreOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Start")]
        public async Task<IHttpActionResult> StartOrder(long OrderId)
        {
            CurrentUserId = User.Identity.GetUserId();
            Driver = db.Drivers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && w.DriverId == Driver.Id && w.Status == StoreOrderStatus.Preparing);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.Status =  StoreOrderStatus.Delivering;
            db.SaveChanges();
            var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
            OrdersHub.Clients.Group("StoreOrdersDashboard").update(Order.Id);
            string Title = $"طلبكم رقم {Order.Code} جارى توصيله الان";
            string Body = $"طلبكم الان مع احد السائقين وجارى توصيله اليكم"; 
            Notifications.SendWebNotification($"جاري توصيل الأن الطلب رقم {Order.Code}", "الطلب الأن مع أحد السائقين وجارى توصيلة", "1b8381b5 -1890-416e-a8d0-c8446535ba65", -1, "Amdin", Order.Id, NotificationType.WebStoreOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Finish")]
        public async Task<IHttpActionResult> FinishOrder(long OrderId)
        {
            CurrentUserId = User.Identity.GetUserId();
            Driver = db.Drivers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.StoreOrders.Include(i=>i.Items).FirstOrDefault(w => w.Id == OrderId && w.IsDeleted == false && w.DriverId == Driver.Id && w.Status == StoreOrderStatus.Delivering);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                  
                    ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "6cbac676-8e4a-4e01-ace4-43814d464519");
                    Admin.Wallet += (Order.DeliveryFees * ((decimal)Driver.Profit / (decimal)100.0));
                    db.SaveChanges();
                    var items = db.StoreOrderItems.Include(x => x.Product).Include(x => x.Product.Category)
                            .Where(x => x.OrderId == OrderId).ToList();
                    Store store;
                    foreach (var item in items)
                    {
                        new Store();
                        store = await db.Stores.FirstOrDefaultAsync(x => x.Id == item.Product.Category.StoreId);
                        store.Wallet += (item.SubTotal - ((decimal)item.SubTotal * ((decimal)store.Profit / (decimal)100.0)));
                        db.SaveChanges();
                        Admin.Wallet += ((decimal)item.SubTotal * ((decimal)store.Profit / (decimal)100.0));
                        db.SaveChanges();
                    }
                    Order.Status = StoreOrderStatus.Finished;
                    Order.DeliveredOn = DateTime.Now.ToUniversalTime();
                    db.SaveChanges();
                    if (Order.PaymentMethod == PaymentMethod.Cash || Order.PaymentMethod == PaymentMethod.Wallet)
                    {

                        if (Order.PaymentMethod == PaymentMethod.Wallet)
                        {

                            Order.User.Wallet -= Order.Total;
                            Order.Driver.User.Wallet += (Order.DeliveryFees - (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0)));
                            db.SaveChanges();
                            db.UserWallets.Add(new UserWallet()
                            {
                                StoreOrderId = OrderId,
                                TransactionAmount = Order.Total,
                                TransactionType = TransactionType.UserPaidForStoreOrderByWallet,
                                UserId = Order.UserId
                            });
                            db.SaveChanges();
                            db.UserWallets.Add(new UserWallet()
                            {
                                StoreOrderId = OrderId,
                                TransactionType = TransactionType.AddedByAdminManually,
                                UserId = Driver.UserId,
                                TransactionAmount = (Order.DeliveryFees - (Order.DeliveryFees * ((decimal)Driver.Profit / (decimal)100.0)))
                            });
                            db.SaveChanges();
                        }
                        else
                        {
                            Order.Driver.User.Wallet -= (Order.Total - (Order.DeliveryFees * (((decimal)100 - (decimal)Order.Driver.Profit) / (decimal)100.0)));
                            db.SaveChanges();
                            db.UserWallets.Add(new UserWallet()
                            {
                                StoreOrderId = OrderId,
                                TransactionType = TransactionType.SubtractedByAdminManually,
                                UserId = Driver.UserId,
                                TransactionAmount = (Order.Total - (Order.DeliveryFees * (((decimal)100 - (decimal)Driver.Profit) / (decimal)100.0)))
                            });
                            db.SaveChanges();
                        }
                        Order.IsPaid = true;
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    else
                    {
                        baseResponse.Data = new
                        {
                            Order.PaymentMethod,
                            Order.PaymentUniqueKey
                        };
                        return Ok(baseResponse);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionLogging.SendErrorToText(ex);
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
            Product product;
            foreach (var item in Order.Items.ToList())
            {
                product = new Product();
                product = db.StoreOrderItems.Include(x=>x.Product).FirstOrDefault(x => x.ProductId == item.ProductId).Product;
                product.Inventory -= item.Quantity;
                db.SaveChanges();
            }
            DriversActions.SetDriverAvailability(Driver);
            EmailSender sender = new EmailSender();
            try
            {
                sender.PrepareHTMLandSend(Order, "en");
            }
            catch (Exception) { }
            var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
            OrdersHub.Clients.Group("StoreOrdersDashboard").update(Order.Id);
            Notifications.SendWebNotification("تم توصيل الطلب بنجاح", "قام السائق بتوصيل الطلب للعميل بنجاح ", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.Id, NotificationType.WebStoreOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, "تم توصيل طلبكم", "شكراً لك ونتمنى زيارتك مجدداً", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, "تم توصيل طلبكم", "شكراً لك ونتمنى زيارتك مجدداً", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, "تم توصيل الطلب", "قمت بتوصيل الطلب بنجاح ، شكراً لك", NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, "تم توصيل الطلب", "قمت بتوصيل الطلب بنجاح ، شكراً لك", NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Details")]
        public IHttpActionResult GetOrderDetails(long OrderId, string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            Driver = db.Drivers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.StoreOrders.FirstOrDefault(s => s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));

            DateTime? DeliverDate = null;
            if (Order.DeliveredOn.HasValue == true)
            {
                DeliverDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
            }

            DriverStoreOrderDetailsDTO detailsDTO = new DriverStoreOrderDetailsDTO()
            {
                OrderId = Order.Id,
                Code = Order.Code,
                CreateDate = CreatedOn.ToString("dd MMMM yyyy"),
                CreateTime = CreatedOn.ToString("hh:mm tt"),
                DeliverDate = DeliverDate.HasValue ? DeliverDate.Value.ToString("dd MMMM yyyy") : null,
                DeliverTime = DeliverDate.HasValue ? DeliverDate.Value.ToString("hh:mm tt") : null,
                PaymentMethod = Order.PaymentMethod,
                DeliveryFees = Order.DeliveryFees,
            };
            if (Order.User.ImageUrl != null)
            {
                detailsDTO.UserImage = MediaControl.GetPath(FilePath.Users) + Order.User.ImageUrl;
            }
            switch (Order.PaymentMethod)
            {
                case PaymentMethod.Cash:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "الدفع عند الاستلام" : "Cash on Delivery";
                    detailsDTO.PaymentMethod = PaymentMethod.Cash;
                    break;
                case PaymentMethod.Online:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "اونلاين" : "Online";
                    detailsDTO.PaymentMethod = PaymentMethod.Online;
                    break;
                case PaymentMethod.Wallet:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "الدفع من المحفظة" : "Wallet";
                    detailsDTO.PaymentMethod = PaymentMethod.Wallet;
                    break;
                default:
                    break;
            }

            if (Order.UserAddressId.HasValue == true)
            {
                detailsDTO.UserAddress = new UserAddressDTO()
                {
                    Address = Order.UserAddress.Address,
                    AddressInDetails = Order.UserAddress.AddressInDetails,
                    Apartment = Order.UserAddress.Apartment,
                    BuildingNumber = Order.UserAddress.BuildingNumber,
                    Floor = Order.UserAddress.Floor,
                    Id = Order.UserAddress.Id,
                    Latitude = Order.UserAddress.Latitude,
                    Longitude = Order.UserAddress.Longitude,
                    Name = Order.UserAddress.Name,
                    PhoneNumber = Order.UserAddress.PhoneNumber,
                };
            }

            if (Order.Items != null)
            {
                var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                if (Item != null)
                {
                    var Store = Item.Product.Category.Store;
                    detailsDTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                    detailsDTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                }
                var Product = new Product();
                foreach (var item in Order.Items.Where(d => d.IsDeleted == false))
                {
                    Product = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    BasketItemDTO basketItemDTO = new BasketItemDTO()
                    {
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.DescriptionAr : item.Product.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.NameAr : item.Product.NameEn,
                        BasketItemId = item.Id,
                        Image = item.Product.Images != null && item.Product.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? MediaControl.GetPath(FilePath.Product) + item.Product.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null,
                        Quantity = item.Quantity,
                    };
                    if (lang.ToLower() == "ar")
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? Product.CurrencyAr : "ريال");
                        basketItemDTO.SubTotal = item.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        detailsDTO.Total = Order.Total.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        detailsDTO.SubTotal = Order.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);

                    }
                    else
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? Product.Currency : "ريال");
                        basketItemDTO.SubTotal = item.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        detailsDTO.Total = Order.Total.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        detailsDTO.SubTotal = Order.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);


                    }
                    if (item.SizeId.HasValue == true)
                    {
                        basketItemDTO.Size = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Size.SizeAr : item.Size.SizeEn;
                    }
                    detailsDTO.Items.Add(basketItemDTO);
                }
            }

            if (Order.DriverId.HasValue == true)
            {
                DriverReview DriverReview = null;
                if (Order.DriverReviews != null)
                {
                    DriverReview = db.DriverReviews.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.DriverId == Order.DriverId && s.StoreOrderId == Order.Id);
                }
                if (Order.Status == StoreOrderStatus.Finished)
                {
                    if (DriverReview != null)
                    {
                        detailsDTO.IsUserReviewedDriver = true;
                        detailsDTO.UserDriverRate = DriverReview.Rate;
                        detailsDTO.UserDriverReview = DriverReview.Review;
                    }
                    else
                    {
                        detailsDTO.IsUserReviewedDriver = false;
                        detailsDTO.UserDriverRate = -1;
                    }
                }
            }
            switch (Order.Status)
            {
                case StoreOrderStatus.Placed:
                    detailsDTO.CanCallCustomer = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                    break;
                case StoreOrderStatus.Finished:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                    break;
                case StoreOrderStatus.Delivering:
                    detailsDTO.CanCallCustomer = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "Delivering";
                    break;
                case StoreOrderStatus.Cancelled:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                    break;
                case StoreOrderStatus.Preparing:
                    detailsDTO.CanCallCustomer = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التجهيز" : "Preparing";
                    break;
                case StoreOrderStatus.Rejected:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "مرفوض" : "Rejected";
                    break;
                default:
                    break;
            }

            baseResponse.Data = detailsDTO;
            return Ok(baseResponse);
        }

    }
}