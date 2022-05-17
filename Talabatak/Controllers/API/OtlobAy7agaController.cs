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
using System.Web.Http.ModelBinding;
using System.Data.Entity;

namespace Talabatak.Controllers.API
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/OtlobAy7aga")]
    public class OtlobAy7agaController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public OtlobAy7agaController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("neworderpage")]
        public IHttpActionResult NewOrderPage()
        {
            GetNewOrderPageData();
            return Ok(baseResponse);
        }

        private void GetNewOrderPageData()
        {
            string CurrentUserId = User.Identity.GetUserId();
            OtlobAy7agaNewOrderPageDTO dTO = new OtlobAy7agaNewOrderPageDTO();
            var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);
            if (UserOrder != null)
            {
                dTO.Description = UserOrder.Details;
                dTO.CityId = UserOrder.CityId;
                if (UserOrder.ImageUrl != null)
                {
                    dTO.Image = MediaControl.GetPath(FilePath.Other) + UserOrder.ImageUrl;
                }
            }
            baseResponse.Data = dTO;
        }
        [HttpPost]
        [Route("neworder")]
        public IHttpActionResult UploadOrderImage(OtlobAy7agaNewOrderPageDTO model)
        {
            string CurrentUserId = User.Identity.GetUserId();

            #region Validation
            if (string.IsNullOrEmpty(model.Description) || string.IsNullOrWhiteSpace(model.Description))
            {
                baseResponse.ErrorCode = Errors.OrderDescriptionIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var City = db.Cities.FirstOrDefault(w => w.IsDeleted == false && w.Country.IsDeleted == false && w.Id == model.CityId);
            if (City == null)
            {
                baseResponse.ErrorCode = Errors.CityNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            byte[] Image = null;
            if (!string.IsNullOrEmpty(model.Image))
            {
                try
                {
                    Image = Convert.FromBase64String(model.Image);
                    if (Image == null || Image.Length <= 0)
                    {
                        baseResponse.ErrorCode = Errors.FailedToUpload;
                        return Content(HttpStatusCode.BadRequest, baseResponse);
                    }
                }
                catch (Exception)
                {
                    baseResponse.ErrorCode = Errors.FailedToUpload;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
            }
            #endregion

            try
            {
                var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);
                if (UserOrder == null)
                {
                    OtlobAy7agaOrder order = new OtlobAy7agaOrder()
                    {
                        FinishCode = RandomGenerator.GenerateNumber(1000, 9999),
                        Code = RandomGenerator.GenerateNumber(100000, 999999).ToString(),
                        OrderStatus = OtlobAy7agaOrderStatus.Initialized,
                        UserId = CurrentUserId,
                        CityId = model.CityId,
                        Details = model.Description,
                        EstimatedDeliverTimeInSeconds = 0,
                        EstimatedDeliverDistance = 0,
                        SubTotal = 0,
                        Total = 0,
                        DeliveryFees = 0,
                        IsPaid = false,
                        PaymentMethod = PaymentMethod.Cash,
                    };
                    if (model.Image != null)
                    {
                        order.ImageUrl = MediaControl.Upload(FilePath.Other, Image, MediaType.Image);
                    }
                    db.OtlobAy7agaOrders.Add(order);
                    UserOrder = order;
                }
                else
                {
                    UserOrder.Details = model.Description;
                    UserOrder.CityId = model.CityId;
                    if (Image != null)
                    {
                        UserOrder.ImageUrl = MediaControl.Upload(FilePath.Other, Image, MediaType.Image);
                    }
                }
                db.SaveChanges();
                GetNewOrderPageData();
                return Ok(baseResponse);
            }
            catch (Exception)
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
        }

        [HttpDelete]
        [Route("DeleteImage")]
        public IHttpActionResult DeleteOrderImage()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);
            if (UserOrder != null)
            {
                MediaControl.Delete(FilePath.Other, UserOrder.ImageUrl);
                UserOrder.ImageUrl = null;
                db.SaveChanges();
            }
            GetNewOrderPageData();
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("Checkout")]
        public async Task<IHttpActionResult> Checkout(UserAddressDTO Address)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.OtlobAy7agaOrders.Include(x=>x.Driver).Include(x=>x.Driver.User).FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);

            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserDoesNotHaveOrders;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = ValidateNewUserAddressApi(ModelState);
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Address.Latitude.HasValue == false)
            {
                baseResponse.ErrorCode = Errors.LatitudeFieldIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            if (Address.Longitude.HasValue == false)
            {
                baseResponse.ErrorCode = Errors.LongitudeFieldIsRequired;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                
                    UserAddress userAddress = UserAddressDTO.toUserAddress(Address, CurrentUserId);
                    db.UserAddresses.Add(userAddress);
                    db.SaveChanges();
                    UserOrder.OrderStatus = OtlobAy7agaOrderStatus.Placed;
                    UserOrder.UserAddressId = userAddress.Id;
                    UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
            
               
                    db.SaveChanges();
                    Transaction.Commit();
                    await SMS.SendMessage("", "01024401763", "هناك طلب جديد لأطلب اي حاجه من فضلك قم بزيارة لوحة التحكم");
                    Notifications.SendWebNotification("طلب جديد من أطلب أي حاجه", "جارى البحث عن العروض المناسبة لطلبك", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", UserOrder.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
                    await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "جارى البحث عن العروض المناسبة لطلبك", NotificationType.ApplicationPendingOtlobAy7agaOrdersPage, UserOrder.Id, false, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, "تم استقبال طلبكم", "جارى البحث عن العروض المناسبة لطلبك", NotificationType.ApplicationPendingOtlobAy7agaOrdersPage, UserOrder.Id, false, true);
                    return Ok(baseResponse);
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }

        }

        private Errors ValidateNewUserAddressApi(ModelStateDictionary Model)
        {
            var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            if (ModelErrors.Any(x => x.Contains("Name")))
                return Errors.NameIsRequired;

            if (ModelErrors.Any(x => x.Contains("Address")))
                return Errors.UserAddressIsRequired;

            if (ModelErrors.Any(x => x.Contains("Floor")))
                return Errors.FloorIsRequired;

            if (ModelErrors.Any(x => x.Contains("BuildingNumber")))
                return Errors.BuildingNumberIsRequired;

            if (ModelErrors.Any(x => x.Contains("Apartment")))
                return Errors.ApartmentIsRequired;

            if (ModelErrors.Any(x => x.Contains("PhoneNumber")))
                return Errors.PhoneIsRequiredAndInCorrectFormat;

            return Errors.SomethingWentWrong;
        }

        [HttpDelete]
        [Route("Cancel")]
        public async Task<IHttpActionResult> CancelOrder(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Placed && d.UserId == CurrentUserId && d.Id == OrderId);
            if (UserOrder != null)
            {
                UserOrder.OrderStatus = OtlobAy7agaOrderStatus.Cancelled;
                db.SaveChanges();
                var OrderDrivers = db.OtlobAy7agaOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == OrderId && s.IsAccepted.HasValue == false).ToList();
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                foreach (var driver in OrderDrivers)
                {
                    OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // To remove the order from these drivers page.
                }
                if (UserOrder.DriverId.HasValue == true)
                {
                    string Title = $"تم الغاء الطلب رقم {UserOrder.Code}";
                    string Body = $"نعتذر ، قام العميل بالغاء طلب التوصيل رقم {UserOrder.Code}";
                    Notifications.SendWebNotification($"تم الغاء الطلب رقم {UserOrder.Code}", $"نعتذر ، قام العميل بالغاء طلب التوصيل رقم {UserOrder.Code}", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", UserOrder.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
                    await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.Driver.UserId, Title, Body, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.Driver.UserId, Title, Body, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
                }
            }
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Details")]
        public IHttpActionResult GetOrderDetails(long OrderId, string lang = "en")
        {
            var CurrentUserId = User.Identity.GetUserId();
            var order = db.OtlobAy7agaOrders.FirstOrDefault(s => s.Id == OrderId && s.IsDeleted == false && s.OrderStatus != OtlobAy7agaOrderStatus.Initialized && s.UserId == CurrentUserId);
            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
            UserOtlobAy7agaOrderDTO orderDTO = new UserOtlobAy7agaOrderDTO()
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
                case OtlobAy7agaOrderStatus.Finished:
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                    break;
                case OtlobAy7agaOrderStatus.Cancelled:
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                    break;
                case OtlobAy7agaOrderStatus.Placed:
                    if (order.DriverId.HasValue == true)
                    {
                        orderDTO.CanCallDriver = true;
                        orderDTO.CanChat = true;
                    }
                    orderDTO.CanCancel = true;
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                    break;
                case OtlobAy7agaOrderStatus.Started:
                    orderDTO.CanCancel = true;
                    orderDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "Delivering";
                    if (order.DriverId.HasValue == true)
                    {
                        orderDTO.CanCallDriver = true;
                        orderDTO.CanChat = true;
                    }
                    break;
                default:
                    break;
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
            if (order.DriverId.HasValue == true)
            {
                orderDTO.getDriverOrdersCount = order.Driver.NumberOfCompletedTrips;
                orderDTO.IsHaveDriver = true;
                orderDTO.Driver = DriverDTO.toDeliveryManDTO(order.Driver);
            }
            baseResponse.Data = orderDTO;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("GetPaymentLink")]
        public async Task<IHttpActionResult> GetPaymentLink()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserDoesNotHaveOrders;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Link = await PaymentGateway.GenerateOtlobAy7agaPaymentLink(UserOrder);
            if (Link != null)
            {
                baseResponse.Data = new
                {
                    Link
                };
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }
    }
}