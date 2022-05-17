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
    [RoutePrefix("api/DriverOtlobAy7aga")]
    public class OtlobAy7agaDriversController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public OtlobAy7agaDriversController()
        {
            baseResponse = new BaseResponseDTO();
        }
        [HttpGet]
        [Route("DeliveryFees")]
        public async Task<IHttpActionResult> GetEstimatedDeliveryFees(long OrderId, long ApplicationId, string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            var Order = db.OtlobAy7agaOrderDrivers.FirstOrDefault(s => s.Order.IsDeleted == false && s.Order.OrderStatus == OtlobAy7agaOrderStatus.Placed && s.IsDeleted == false && s.Id == ApplicationId && s.OrderId == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.Order.DriverId.HasValue == true)
            {
                baseResponse.ErrorCode = Errors.OrderAcceptedByAnotherDriver;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Driver.User.Latitude.HasValue == false || Driver.User.Longitude.HasValue == false)
            {
                baseResponse.ErrorCode = Errors.DriverCoordinatesUnknown;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            var Estimated = await Distance.GetEstimatedDataBetweenTwoLocations(Driver.User.Latitude.Value, Driver.User.Longitude.Value, Order.Order.UserAddress.Latitude.Value, Order.Order.UserAddress.Longitude.Value);
            var TotalEstimatedDistance = Estimated.TotalEstimatedDistance / 1000.00;
            baseResponse.Data = new
            {
                Kilometers = TotalEstimatedDistance.ToString("N2") + " " + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " كم" : " KM"),
                DeliveryFees = OtlobAy7agaOrderActions.CalculateDeliveryFees(Estimated.TotalEstimatedDistance).ToString("N2") + " " + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " ريال" : "LE"),
            };
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Accept")]
        public async Task<IHttpActionResult> AcceptPendingOrder(long OrderId, long ApplicationId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            var Order = db.OtlobAy7agaOrderDrivers.FirstOrDefault(s => s.Order.IsDeleted == false && 
            (s.Order.OrderStatus == OtlobAy7agaOrderStatus.Placed|| s.Order.OrderStatus == OtlobAy7agaOrderStatus.Initialized) && s.IsDeleted == false && s.Id == ApplicationId && s.OrderId == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
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
            var Estimated = await Distance.GetEstimatedDataBetweenTwoLocations(Order.Driver.User.Latitude.Value,
                    Order.Driver.User.Longitude.Value, Order.Order.UserAddress.Latitude.Value, Order.Order.UserAddress.Longitude.Value);


            Order.IsAccepted = true;
            Order.AcceptedOn = DateTime.Now.ToUniversalTime();
            Order.Order.DriverId = Driver.Id;
            Order.Order.EstimatedDeliverDistance = Estimated.TotalEstimatedDistance;
            Order.Order.EstimatedDeliverTimeInSeconds = Estimated.TotalEstimatedTime;
            Order.Order.DeliveryFees = OtlobAy7agaOrderActions.CalculateDeliveryFees(Estimated.TotalEstimatedDistance);

            var otlob = db.OtlobAy7agaOrders.FirstOrDefault(x => x.Id == Order.OrderId);
            otlob.DeliveryProfit = Driver.Profit;
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            var OrderDrivers = db.OtlobAy7agaOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == OrderId && s.IsAccepted.HasValue == false).ToList();
            var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
            foreach (var driver in OrderDrivers)
            {
                OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // To remove the order from these drivers page.
            }

            string UserTitle = $"السائق {Driver.User.Name} استلم طلبك";
            string UserBody = $"تم اسناد طلبك رقم {Order.Order.Code} للسائق {Driver.User.Name}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            Notifications.SendWebNotification($"استلم {Driver.User.Name} طلب جديد", "السائق متجهه الأن للعميل والبداء فى التوصيل", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.OrderId, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            string DriverTitle = $"تم الموافقه على الطلب رقم {Order.Order.Code}";
            string DriverBody = $"يمكنك الان التواصل مع العميل لتوصيل الطلب";
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Reject")]
        public async Task<IHttpActionResult> RejectPendingOrder(long OrderId, long ApplicationId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return  Content(HttpStatusCode.NotFound, baseResponse);
            }
            var Order = db.OtlobAy7agaOrderDrivers.FirstOrDefault(s => s.Order.IsDeleted == false && s.Order.OrderStatus == OtlobAy7agaOrderStatus.Placed && s.IsDeleted == false && s.Id == ApplicationId && s.OrderId == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.IsAccepted = false;
            Order.RejectedOn = DateTime.Now.ToUniversalTime();
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Start")]
        public async Task<IHttpActionResult> StartOrder(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.OtlobAy7agaOrders.FirstOrDefault(s => s.OrderStatus == OtlobAy7agaOrderStatus.Placed && s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.DriverId != Driver.Id)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            string UserTitle = $"السائق {Driver.User.Name} قام بالبدأ بالتوصيل";
            string UserBody = $"قام السائق {Driver.User.Name} بالتحرك لتوصيل طلبك رقم {Order.Code}";
            Notifications.SendWebNotification(UserTitle, $"قام السائق {Driver.User.Name} بالتحرك لتوصيل الطلب رقم {Order.Code}", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            Order.OrderStatus = OtlobAy7agaOrderStatus.Started;
            CRUD<OtlobAy7agaOrder>.Update(Order);
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Cancel")]
        public async Task<IHttpActionResult> CancelPendingOrder(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            var Order = db.OtlobAy7agaOrders.FirstOrDefault(s => (s.OrderStatus == OtlobAy7agaOrderStatus.Placed || s.OrderStatus == OtlobAy7agaOrderStatus.Initialized || s.OrderStatus == OtlobAy7agaOrderStatus.Started) && s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.DriverId != Driver.Id)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var OrderDriver = db.OtlobAy7agaOrderDrivers.FirstOrDefault(s => s.IsDeleted == false && s.IsAccepted == true && s.DriverId == Driver.Id && s.OrderId == OrderId);
            if (OrderDriver != null)
            {
                OrderDriver.IsAccepted = false;
                OrderDriver.RejectedOn = DateTime.Now.ToUniversalTime();
            }
            string UserTitle = $"السائق {Driver.User.Name} قام بالانسحاب من الطلب";
            string UserBody = $"قام السائق {Driver.User.Name} بالانسحاب من توصيل طلبك رقم {Order.Code}";
            Notifications.SendWebNotification("تم اللغاء الطلب", "العميل قام بالغاء الطلب ", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", OrderId, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            Order.OrderStatus = OtlobAy7agaOrderStatus.Placed;
            CRUD<OtlobAy7agaOrder>.Update(Order);
            Order.DriverId = null;
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("SendFinishCode")]
        public async Task<IHttpActionResult> SendFinishCode(long OrderId, decimal Cost)
        {
          
             var CurrentUserId = User.Identity.GetUserId();         
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            var Order = db.OtlobAy7agaOrders.FirstOrDefault(s => (s.OrderStatus == OtlobAy7agaOrderStatus.Started || s.OrderStatus == OtlobAy7agaOrderStatus.Initialized || s.OrderStatus == OtlobAy7agaOrderStatus.Placed) && s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            if (Order.DriverId != Driver.Id)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            Order.SubTotal = Cost;
            Order.Total = Cost + Order.DeliveryFees;
            db.SaveChanges();
            var user = db.Users.FirstOrDefault(d => d.Id == CurrentUserId);
            Order.OrderStatus = OtlobAy7agaOrderStatus.Finished;
            Order.IsPaid = true;
            db.SaveChanges();
            Driver.NumberOfCompletedTrips += 1;
            db.SaveChanges();
            ApplicationUser Admin = db.Users.FirstOrDefault(d => d.Id == "6cbac676-8e4a-4e01-ace4-43814d464519");
            Admin.Wallet += (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            user.Wallet -= (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            string UserTitle = $"تم توصيل طلبكم بنجاح";
            string UserBody = $"شكراً لاستخدام خدمه [اطلب اى حاجه] وننتظر طلباتكم القادمة";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string DriverTitle = $"تم توصيل الطلب رقم {Order.Code} بنجاح";
            string DriverBody = $"لقد قمت بتوصيل الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            Notifications.SendWebNotification(DriverTitle, DriverBody, "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            return Ok(baseResponse);          
        }

        [HttpGet]
        [Route("ResendFinishCode")]
        public async Task<IHttpActionResult> ResendFinishCode(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.OtlobAy7agaOrders.FirstOrDefault(s => s.OrderStatus == OtlobAy7agaOrderStatus.Started && s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.DriverId != Driver.Id)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var user = db.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            Order.OrderStatus = OtlobAy7agaOrderStatus.Finished;
            Order.IsPaid = true;
            db.SaveChanges();
            Driver.NumberOfCompletedTrips += 1;
            db.SaveChanges();
            ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "6cbac676-8e4a-4e01-ace4-43814d464519");
            Admin.Wallet += (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            user.Wallet -= (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            string UserTitle = $"تم توصيل طلبكم بنجاح";
            string UserBody = $"شكراً لاستخدام خدمه [اطلب اى حاجه] وننتظر طلباتكم القادمة";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string DriverTitle = $"تم توصيل الطلب رقم {Order.Code} بنجاح";
            string DriverBody = $"لقد قمت بتوصيل الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            Notifications.SendWebNotification(DriverTitle, DriverBody, "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Finish")]
        public async Task<IHttpActionResult> FinishOrder(long OrderId, int Code)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            var user = db.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.OtlobAy7agaOrders.Include(x=>x.Driver).Include(x=>x.Driver.User)
                .FirstOrDefault(s => s.OrderStatus == OtlobAy7agaOrderStatus.Started && s.IsDeleted == false && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.DriverId != Driver.Id)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Order.FinishCode != Code)
            {
                baseResponse.ErrorCode = Errors.InvalidFinishCode;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            Order.OrderStatus = OtlobAy7agaOrderStatus.Finished;
            Order.IsPaid = true;
            db.SaveChanges();
            Driver.NumberOfCompletedTrips += 1;
            db.SaveChanges();
            ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "6cbac676-8e4a-4e01-ace4-43814d464519");
            Admin.Wallet += (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            user.Wallet -= (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.DeliveryFees * ((decimal)Order.Driver.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            DriversActions.SetDriverAvailability(Driver);
            string UserTitle = $"تم توصيل طلبكم بنجاح";
            string UserBody = $"شكراً لاستخدام خدمه [اطلب اى حاجه] وننتظر طلباتكم القادمة";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string DriverTitle = $"تم توصيل الطلب رقم {Order.Code} بنجاح";
            string DriverBody = $"لقد قمت بتوصيل الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            Notifications.SendWebNotification(DriverTitle, DriverBody , "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", Order.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, DriverTitle, DriverBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);

            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Details")]
        public async Task<IHttpActionResult> GetOrderDetails(long OrderId, string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var order = db.OtlobAy7agaOrders.FirstOrDefault(s => s.Id == OrderId && s.IsDeleted == false && s.OrderStatus != OtlobAy7agaOrderStatus.Initialized);
            if (order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
            DriverOtlobAy7agaOrderDTO orderDTO = new DriverOtlobAy7agaOrderDTO()
            {
                Code = order.Code,
                PaymentMethod = order.PaymentMethod,
                CreationDate = CreatedOn.ToString("dd MMMM yyyy"),
                CreationTime = FormatDate.TimeToString(lang, CreatedOn.TimeOfDay),
                DeliveryFees = order.DeliveryFees,
                Description = order.Details,
                Distance = (order.EstimatedDeliverDistance / 1000).ToString("N0") + " " + (!string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? " كم" : " KM"),
                Id = order.Id,
                Image = order.ImageUrl != null ? MediaControl.GetPath(FilePath.Other) + order.ImageUrl : null,
                SubTotal = order.SubTotal,
                Total = order.Total,
                City = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? order.City.NameAr : order.City.NameEn,
            };
            switch (order.OrderStatus)
            {
                case OtlobAy7agaOrderStatus.Placed:
                    orderDTO.CanCallCustomer = true;
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                    break;
                case OtlobAy7agaOrderStatus.Cancelled:
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                    break;
                case OtlobAy7agaOrderStatus.Started:
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "Delivering";
                    orderDTO.CanCallCustomer = true;
                    break;
                case OtlobAy7agaOrderStatus.Finished:
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                    break;
                default:
                    break;
            }
            if (order.User.ImageUrl != null)
            {
                orderDTO.UserImage = MediaControl.GetPath(FilePath.Users) + order.User.ImageUrl;
            }
            switch (order.PaymentMethod)
            {
                case PaymentMethod.Cash:
                    orderDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "الدفع عند الاستلام" : "Cash on Delivery";
                    break;
                case PaymentMethod.Online:
                    orderDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "اونلاين" : "Online";
                    break;
                case PaymentMethod.Wallet:
                    orderDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "من المحفظة" : "Wallet";
                    break;
                default:
                    break;
            }
            if (order.UserAddressId.HasValue == true)
            {
                orderDTO.UserAddress = new UserAddressDTO()
                {
                    Address = order.UserAddress.Address,
                    AddressInDetails = order.UserAddress.AddressInDetails,
                    Apartment = order.UserAddress.Apartment,
                    BuildingNumber = order.UserAddress.BuildingNumber,
                    Floor = order.UserAddress.Floor,
                    Id = order.UserAddress.Id,
                    Latitude = order.UserAddress.Latitude,
                    Longitude = order.UserAddress.Longitude,
                    Name = order.UserAddress.Name,
                    PhoneNumber = order.UserAddress.PhoneNumber,
                };
            }
            baseResponse.Data = orderDTO;
            return Ok(baseResponse);
        }
    }
}