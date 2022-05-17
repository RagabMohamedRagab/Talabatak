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

namespace Talabatak.Controllers.API
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/common/Orders")]
    public class CommonOrdersController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private string CurrentUserId;

        public CommonOrdersController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("currentuser")]
        public IHttpActionResult GetCurrentUserOrders(string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var Orders = db.StoreOrders.Where(d => d.UserId == CurrentUserId && (d.Status == StoreOrderStatus.Placed || d.Status == StoreOrderStatus.Delivering || d.Status == StoreOrderStatus.Preparing) && d.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            var OtlobOrders = db.OtlobAy7agaOrders.Where(s => s.IsDeleted == false && (s.OrderStatus == OtlobAy7agaOrderStatus.Placed || s.OrderStatus == OtlobAy7agaOrderStatus.Started) && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList();
            var WorkerOrders = db.JobOrders.Where(s => s.IsDeleted == false && (s.Status == JobOrderStatus.Placed || s.Status == JobOrderStatus.AcceptedByWorker) && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList();
            List<CurrentUserOrderDTO> orderDTOs = new List<CurrentUserOrderDTO>();
            foreach (var order in Orders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new CurrentUserOrderDTO()
                {
                    IsStoreOrder = true,
                    OrderType = OrderType.StoreOrder,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.Total,
                    CreatedOn = order.CreatedOn
                };
                if (order.UserAddressId.HasValue == true)
                {
                    dTO.Address = order.UserAddress.Address;
                }
                var Item = order.Items.FirstOrDefault(w => w.IsDeleted == false);
                if (Item != null)
                {
                    var Store = Item.Product.Category.Store;
                    dTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                    dTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                }
                switch (order.Status)
                {
                    case StoreOrderStatus.Placed:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                        break;
                    case StoreOrderStatus.Delivering:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                        break;
                    case StoreOrderStatus.Preparing:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التحضير" : "Preparing";
                        break;
                    default:
                        break;
                }
                if (order.DriverId.HasValue == true)
                {
                    dTO.IsHaveDriver = true;
                    dTO.CanCallDriver = true;
                    dTO.CanChat = true;
                }
                orderDTOs.Add(dTO);
            }

            foreach (var order in OtlobOrders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new CurrentUserOrderDTO()
                {
                    IsStoreOrder = false,
                    OrderType = OrderType.OtlobAy7aga,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.Total,
                    DeliveryFees = order.DeliveryFees,
                    SubTotal = order.SubTotal,
                    CreatedOn = order.CreatedOn
                };
                if (order.UserAddressId.HasValue == true)
                {
                    dTO.Address = order.UserAddress.Address;
                }
                if (!string.IsNullOrEmpty(order.ImageUrl))
                {
                    dTO.OtlobAy7agaImageUrl = MediaControl.GetPath(FilePath.Other) + order.ImageUrl;
                }
                switch (order.OrderStatus)
                {
                    case OtlobAy7agaOrderStatus.Placed:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                        break;
                    case OtlobAy7agaOrderStatus.Started:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                        break;
                    default:
                        break;
                }
                if (order.DriverId.HasValue == true)
                {
                    dTO.IsHaveDriver = true;
                    dTO.CanCallDriver = true;
                    dTO.CanChat = true;
                }
                orderDTOs.Add(dTO);
            }

            foreach (var order in WorkerOrders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new CurrentUserOrderDTO()
                {
                    OrderType = OrderType.JobOrder,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.TotalPrice,
                    DeliveryFees = 0,
                    SubTotal = 0,
                    Address = order.Address,
                    CreatedOn = order.CreatedOn
                };
                if (order.Images != null)
                {
                    var orderImage = order.Images.FirstOrDefault(w => w.IsDeleted == false);
                    if (orderImage != null)
                    {
                        dTO.JobOrderImageUrl = MediaControl.GetPath(FilePath.Other) + orderImage.ImageUrl;
                    }
                }
                switch (order.Status)
                {
                    case JobOrderStatus.Placed:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                        break;
                    case JobOrderStatus.AcceptedByWorker:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم موافقه العامل" : "Accepted by worker";
                        break;
                    default:
                        break;
                }
                if (order.Status == JobOrderStatus.AcceptedByWorker)
                {
                    dTO.IsHaveDriver = true;
                    dTO.CanCallDriver = true;
                    dTO.CanChat = true;
                }
                orderDTOs.Add(dTO);
            }
            orderDTOs = orderDTOs.OrderByDescending(w => w.CreatedOn).ToList();
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("previoususer")]
        public IHttpActionResult GetPreviousUserOrders(string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var Orders = db.StoreOrders.Where(d => d.UserId == CurrentUserId && (d.Status == StoreOrderStatus.Cancelled || d.Status == StoreOrderStatus.Finished || d.Status == StoreOrderStatus.Rejected) && d.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            var OtlobOrders = db.OtlobAy7agaOrders.Where(s => s.IsDeleted == false && (s.OrderStatus == OtlobAy7agaOrderStatus.Cancelled || s.OrderStatus == OtlobAy7agaOrderStatus.Finished) && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList();
            var JobOrders = db.JobOrders.Where(s => s.IsDeleted == false && (s.Status == JobOrderStatus.CancelledByAdmin || s.Status == JobOrderStatus.CancelledByUser || s.Status == JobOrderStatus.Finished || s.Status == JobOrderStatus.RejectedByWorker) && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList();
            List<PreviousUserOrderDTO> orderDTOs = new List<PreviousUserOrderDTO>();
            foreach (var order in Orders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new PreviousUserOrderDTO()
                {
                    IsStoreOrder = true,
                    OrderType = OrderType.StoreOrder,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.Total,
                    CreatedOn = order.CreatedOn
                };
                if (order.UserAddressId.HasValue == true)
                {
                    dTO.Address = order.UserAddress.Address;
                }
                var Item = order.Items.FirstOrDefault(w => w.IsDeleted == false);
                if (Item != null)
                {
                    var Store = Item.Product.Category.Store;
                    dTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                    dTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                }
                switch (order.Status)
                {
                    case StoreOrderStatus.Cancelled:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                        break;
                    case StoreOrderStatus.Finished:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                        break;
                    case StoreOrderStatus.Rejected:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم رفضه" : "Rejected";
                        break;
                    default:
                        break;
                }
                orderDTOs.Add(dTO);
            }

            foreach (var order in OtlobOrders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new PreviousUserOrderDTO()
                {
                    IsStoreOrder = false,
                    OrderType = OrderType.OtlobAy7aga,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.Total,
                    DeliveryFees = order.DeliveryFees,
                    SubTotal = order.SubTotal,
                    CreatedOn = order.CreatedOn
                };
                if (order.UserAddressId.HasValue == true)
                {
                    dTO.Address = order.UserAddress.Address;
                }
                if (!string.IsNullOrEmpty(order.ImageUrl))
                {
                    dTO.OtlobAy7agaImageUrl = MediaControl.GetPath(FilePath.Other) + order.ImageUrl;
                }
                switch (order.OrderStatus)
                {
                    case OtlobAy7agaOrderStatus.Finished:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                        break;
                    case OtlobAy7agaOrderStatus.Cancelled:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                        break;
                    default:
                        break;
                }
                orderDTOs.Add(dTO);
            }

            foreach (var order in JobOrders)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var dTO = new PreviousUserOrderDTO()
                {
                    OrderType = OrderType.JobOrder,
                    OrderCode = order.Code,
                    OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                    OrderId = order.Id,
                    Total = order.TotalPrice,
                    DeliveryFees = 0,
                    SubTotal = 0,
                    Address = order.Address,
                    CreatedOn = order.CreatedOn
                };
                if (order.Images != null)
                {
                    var orderImage = order.Images.FirstOrDefault(w => w.IsDeleted == false);
                    if (orderImage != null)
                    {
                        dTO.JobOrderImageUrl = MediaControl.GetPath(FilePath.Other) + orderImage.ImageUrl;
                    }
                }
                switch (order.Status)
                {
                    case JobOrderStatus.CancelledByAdmin:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء من الاداره" : "Cancelled by Talabatak";
                        break;
                    case JobOrderStatus.CancelledByUser:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                        break;
                    case JobOrderStatus.Finished:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الانتهاء" : "Finished";
                        break;
                    case JobOrderStatus.RejectedByWorker:
                        dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الرفض من العامل" : "Reject by worker";
                        break;
                    default:
                        break;
                }
                orderDTOs.Add(dTO);
            }
            orderDTOs = orderDTOs.OrderByDescending(w => w.CreatedOn).ToList();
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("currentdriver")]
        public IHttpActionResult GetCurrentWorkerOrders(string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId);
            var Worker = db.Workers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId);
            if (Driver == null && Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            List<CurrentWorkerOrderDTO> orderDTOs = new List<CurrentWorkerOrderDTO>();
            if (Driver != null)
            {
                // Pending Store Orders
                var PendingStoreOrders = db.StoreOrderDrivers.Where(s => s.Order.IsDeleted == false && s.Order.Status == StoreOrderStatus.Preparing
              && s.Order.DriverId.HasValue == false && s.IsDeleted == false &&
              s.IsAccepted.HasValue == false && s.DriverId == Driver.Id).OrderByDescending(w => w.CreatedOn).ToList();
                // Current Store Orders
                var CurrentStoreOrders = db.StoreOrders.Where(d => (d.Status == StoreOrderStatus.Delivering || d.Status == StoreOrderStatus.Preparing)
                && d.IsDeleted == false && d.DriverId == Driver.Id).OrderByDescending(s => s.CreatedOn).ToList();
                // Pending Otlob Ay 7aga Orders
                var PendingOtlobOrders = db.OtlobAy7agaOrderDrivers.Where(d => d.IsAccepted.HasValue == false && d.IsDeleted == false && d.DriverId == Driver.Id &&
                d.Order.IsDeleted == false && d.Order.OrderStatus == OtlobAy7agaOrderStatus.Placed && d.Order.DriverId.HasValue == false)
                    .OrderByDescending(d => d.CreatedOn).ToList();
                // Current Otlob Ay 7aga Orders
                var CurrentOtlobOrders = db.OtlobAy7agaOrders.Where(s => s.IsDeleted == false && (s.OrderStatus == OtlobAy7agaOrderStatus.Placed || s.OrderStatus == OtlobAy7agaOrderStatus.Started) && s.DriverId == Driver.Id).OrderByDescending(s => s.CreatedOn).ToList();
                //*******************************************************//
                foreach (var order in PendingStoreOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new CurrentWorkerOrderDTO()
                    {
                        IsStoreOrder = true,
                        OrderType = OrderType.StoreOrder,
                        IsPending = true,
                        ApplicationId = order.Id,
                        OrderCode = order.Order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.OrderId,
                        Total = order.Order.Total,
                        SubTotal = order.Order.SubTotal,
                        DeliveryFees = order.Order.DeliveryFees,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.Order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.Order.UserAddress.Address;
                        dTO.ClientLatitude = order.Order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.Order.UserAddress.Longitude;
                    }
                    var Item = order.Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                    if (Item != null)
                    {
                        var Store = Item.Product.Category.Store;
                        dTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                        dTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                        dTO.StoreLatitude = Store.Latitude;
                        dTO.StoreLongitude = Store.Longitude;
                    }
                    switch (order.Order.Status)
                    {
                        case StoreOrderStatus.Placed:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Placed;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                            break;
                        case StoreOrderStatus.Delivering:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Delivering;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                            break;
                        case StoreOrderStatus.Preparing:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Preparing;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التحضير" : "Preparing";
                            break;
                        default:
                            break;
                    }
                    if (orderDTOs.Any(w => w.OrderId == order.OrderId && w.OrderType == OrderType.StoreOrder) == false)
                    {
                        orderDTOs.Add(dTO);
                    }
                }
                //************************************//
                foreach (var order in CurrentStoreOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new CurrentWorkerOrderDTO()
                    {
                        IsStoreOrder = true,
                        OrderType = OrderType.StoreOrder,
                        IsPending = false,
                        ApplicationId = null,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.Total,
                        SubTotal = order.SubTotal,
                        DeliveryFees = order.DeliveryFees,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.UserAddress.Address;
                        dTO.ClientLatitude = order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.UserAddress.Longitude;
                    }
                    var Item = order.Items.FirstOrDefault(w => w.IsDeleted == false);
                    if (Item != null)
                    {
                        var Store = Item.Product.Category.Store;
                        dTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                        dTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                        dTO.StoreLatitude = Store.Latitude;
                        dTO.StoreLongitude = Store.Longitude;
                    }
                    switch (order.Status)
                    {
                        case StoreOrderStatus.Placed:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Placed;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                            break;
                        case StoreOrderStatus.Delivering:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Delivering;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                            break;
                        case StoreOrderStatus.Preparing:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Preparing;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التحضير" : "Preparing";
                            break;
                        default:
                            break;
                    }
                    if (orderDTOs.Any(w => w.OrderId == order.Id && w.OrderType == OrderType.StoreOrder) == false)
                    {
                        orderDTOs.Add(dTO);
                    }
                }
                //************************************//
                foreach (var order in PendingOtlobOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new CurrentWorkerOrderDTO()
                    {
                        IsStoreOrder = false,
                        OrderType = OrderType.OtlobAy7aga,
                        IsPending = true,
                        ApplicationId = order.Id,
                        OrderCode = order.Order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.OrderId,
                        Total = order.Order.Total,
                        SubTotal = order.Order.SubTotal,
                        DeliveryFees = order.Order.DeliveryFees,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.Order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.Order.UserAddress.Address;
                        dTO.ClientLatitude = order.Order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.Order.UserAddress.Longitude;
                    }
                    if (!string.IsNullOrEmpty(order.Order.ImageUrl))
                    {
                        dTO.OtlobAy7agaImageUrl = MediaControl.GetPath(FilePath.Other) + order.Order.ImageUrl;
                    }
                    switch (order.Order.OrderStatus)
                    {
                        case OtlobAy7agaOrderStatus.Placed:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Placed;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                            break;
                        case OtlobAy7agaOrderStatus.Started:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Started;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                            break;
                        default:
                            break;
                    }
                    if (orderDTOs.Any(w => w.OrderId == order.OrderId && w.OrderType == OrderType.OtlobAy7aga) == false)
                    {
                        orderDTOs.Add(dTO);
                    }
                }
                //************************************//
                foreach (var order in CurrentOtlobOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new CurrentWorkerOrderDTO()
                    {
                        IsStoreOrder = false,
                        OrderType = OrderType.OtlobAy7aga,
                        IsPending = false,
                        ApplicationId = null,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.Total,
                        SubTotal = order.SubTotal,
                        DeliveryFees = order.DeliveryFees,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.UserAddress.Address;
                        dTO.ClientLatitude = order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.UserAddress.Longitude;
                    }
                    if (!string.IsNullOrEmpty(order.ImageUrl))
                    {
                        dTO.OtlobAy7agaImageUrl = MediaControl.GetPath(FilePath.Other) + order.ImageUrl;
                    }
                    switch (order.OrderStatus)
                    {
                        case OtlobAy7agaOrderStatus.Placed:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Placed;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                            break;
                        case OtlobAy7agaOrderStatus.Started:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Started;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "On way";
                            break;
                        default:
                            break;
                    }
                    if (orderDTOs.Any(w => w.OrderId == order.Id && w.OrderType == OrderType.OtlobAy7aga) == false)
                    {
                        orderDTOs.Add(dTO);
                    }
                }
            }

            if (Worker != null)
            {
                // Worker Orders
                var CurrentWorkerOrders = db.JobOrders.Where(s => s.IsDeleted == false && (s.Status == JobOrderStatus.Placed || s.Status == JobOrderStatus.AcceptedByWorker) && s.WorkerId == Worker.Id).OrderByDescending(s => s.CreatedOn).ToList();
                foreach (var order in CurrentWorkerOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new CurrentWorkerOrderDTO()
                    {
                        OrderType = OrderType.JobOrder,
                        IsPending = order.Status == JobOrderStatus.Placed ? true : false,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.TotalPrice,
                        SubTotal = 0,
                        DeliveryFees = 0,
                        Address = order.Address,
                        ClientLatitude = order.Latitude,
                        ClientLongitude = order.Longitude,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.Images != null)
                    {
                        var orderImage = order.Images.FirstOrDefault(w => w.IsDeleted == false);
                        if (orderImage != null)
                        {
                            dTO.JobOrderImageUrl = MediaControl.GetPath(FilePath.Other) + orderImage.ImageUrl;
                        }
                    }
                    switch (order.Status)
                    {
                        case JobOrderStatus.Placed:
                            dTO.OrderStatusId = (int)JobOrderStatus.Placed;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                            break;
                        case JobOrderStatus.AcceptedByWorker:
                            dTO.OrderStatusId = (int)JobOrderStatus.AcceptedByWorker;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم قبوله" : "Accepted";
                            break;
                        default:
                            break;
                    }
                    orderDTOs.Add(dTO);
                }
            }
            orderDTOs = orderDTOs.OrderByDescending(w => w.CreatedOn).ToList();
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("previousdriver")]
        public IHttpActionResult GetPreviousWorkerOrders(string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId);
            var Worker = db.Workers.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId);
            if (Driver == null && Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            List<PreviousDriverOrderDTO> orderDTOs = new List<PreviousDriverOrderDTO>();
            if (Driver != null)
            {
                var StoreOrders = db.StoreOrders.Where(d => d.IsDeleted == false && d.DriverId == Driver.Id && (d.Status == StoreOrderStatus.Cancelled || d.Status == StoreOrderStatus.Finished || d.Status == StoreOrderStatus.Rejected)).OrderByDescending(w => w.CreatedOn).ToList();
                var OtlobOrders = db.OtlobAy7agaOrders.Where(s => s.IsDeleted == false && (s.OrderStatus == OtlobAy7agaOrderStatus.Cancelled || s.OrderStatus == OtlobAy7agaOrderStatus.Finished) && s.DriverId == Driver.Id).OrderByDescending(s => s.CreatedOn).ToList();

                foreach (var order in StoreOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new PreviousDriverOrderDTO()
                    {
                        IsStoreOrder = true,
                        OrderType = OrderType.StoreOrder,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.Total,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.UserAddress.Address;
                        dTO.ClientName = order.UserAddress.Name;
                        dTO.ClientLatitude = order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.UserAddress.Longitude;
                    }
                    var Item = order.Items.FirstOrDefault(w => w.IsDeleted == false);
                    if (Item != null)
                    {
                        var Store = Item.Product.Category.Store;
                        dTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                        dTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                        dTO.StoreLatitude = Store.Latitude;
                        dTO.StoreLongitude = Store.Longitude;
                    }
                    switch (order.Status)
                    {
                        case StoreOrderStatus.Cancelled:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Cancelled;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                            break;
                        case StoreOrderStatus.Finished:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Finished;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                            break;
                        case StoreOrderStatus.Rejected:
                            dTO.OrderStatusId = (int)StoreOrderStatus.Rejected;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم رفضه" : "Rejected";
                            break;
                        default:
                            break;
                    }
                    orderDTOs.Add(dTO);
                }

                foreach (var order in OtlobOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new PreviousDriverOrderDTO()
                    {
                        IsStoreOrder = false,
                        OrderType = OrderType.OtlobAy7aga,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.Total,
                        DeliveryFees = order.DeliveryFees,
                        SubTotal = order.SubTotal,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.UserAddressId.HasValue == true)
                    {
                        dTO.Address = order.UserAddress.Address;
                        dTO.ClientName = order.UserAddress.Name;
                        dTO.ClientLatitude = order.UserAddress.Latitude;
                        dTO.ClientLongitude = order.UserAddress.Longitude;
                    }
                    if (!string.IsNullOrEmpty(order.ImageUrl))
                    {
                        dTO.OtlobAy7agaImageUrl = MediaControl.GetPath(FilePath.Other) + order.ImageUrl;
                    }
                    switch (order.OrderStatus)
                    {
                        case OtlobAy7agaOrderStatus.Finished:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Finished;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                            break;
                        case OtlobAy7agaOrderStatus.Cancelled:
                            dTO.OrderStatusId = (int)OtlobAy7agaOrderStatus.Cancelled;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                            break;
                        default:
                            break;
                    }
                    orderDTOs.Add(dTO);
                }
            }

            if (Worker != null)
            {
                var JobOrders = db.JobOrders.Where(d => d.IsDeleted == false && d.WorkerId == Worker.Id && (d.Status == JobOrderStatus.CancelledByAdmin || d.Status == JobOrderStatus.CancelledByUser || d.Status == JobOrderStatus.RejectedByWorker || d.Status == JobOrderStatus.Finished)).OrderByDescending(w => w.CreatedOn).ToList();

                foreach (var order in JobOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var dTO = new PreviousDriverOrderDTO()
                    {
                        IsStoreOrder = false,
                        OrderType = OrderType.JobOrder,
                        OrderCode = order.Code,
                        OrderDate = CreatedOn.ToString("dd MMM yyyy, hh:mm tt"),
                        OrderId = order.Id,
                        Total = order.TotalPrice,
                        Address = order.Address,
                        ClientName = order.Name,
                        ClientLatitude = order.Latitude,
                        ClientLongitude = order.Longitude,
                        CreatedOn = order.CreatedOn
                    };
                    if (order.Images != null)
                    {
                        var orderImage = order.Images.FirstOrDefault(w => w.IsDeleted == false);
                        if (orderImage != null)
                        {
                            dTO.JobOrderImageUrl = MediaControl.GetPath(FilePath.Other) + orderImage.ImageUrl;
                        }
                    }
                    switch (order.Status)
                    {
                        case JobOrderStatus.CancelledByAdmin:
                            dTO.OrderStatusId = (int)JobOrderStatus.CancelledByAdmin;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الغاءه من الاداره" : "Cancelled by Talabatak";
                            break;
                        case JobOrderStatus.CancelledByUser:
                            dTO.OrderStatusId = (int)JobOrderStatus.CancelledByUser;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الغاءه من العميل" : "Cancelled by client";
                            break;
                        case JobOrderStatus.Finished:
                            dTO.OrderStatusId = (int)JobOrderStatus.Finished;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الانتهاء" : "Finished";
                            break;
                        case JobOrderStatus.RejectedByWorker:
                            dTO.OrderStatusId = (int)JobOrderStatus.RejectedByWorker;
                            dTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم رفضه" : "Rejected";
                            break;
                        default:
                            break;
                    }
                    orderDTOs.Add(dTO);
                }
            }
            orderDTOs = orderDTOs.OrderByDescending(w => w.CreatedOn).ToList();
            baseResponse.Data = orderDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("GetChat")]
        public IHttpActionResult GetOrderChat(long OrderId, OrderType OrderType, string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            if (OrderType == OrderType.StoreOrder)
            {
                var Order = db.StoreOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && (w.UserId == CurrentUserId || (w.DriverId.HasValue == true && w.Driver.UserId == CurrentUserId)));
                if (Order != null)
                {
                    var Chat = db.StoreOrderChats.Where(x => x.OrderId == OrderId && (x.FromUserId == CurrentUserId || x.ToUserId == CurrentUserId)).OrderBy(d => d.CreatedOn).ToList();
                    ChatDTO chatDTO = new ChatDTO()
                    {
                        IsStoreOrder = true,
                        OrderId = OrderId,
                        ChatId = OrderId,
                        OrderCode = Order.Code,
                        DriverId = Order.DriverId,
                        DriverPhoneNumber = Order.DriverId.HasValue == true ? Order.Driver.User.PhoneNumber : null,
                        ClientPhoneNumber = Order.UserAddressId.HasValue == true ? Order.UserAddress.PhoneNumber : Order.User.PhoneNumber,
                        CanChat = true,
                        CanCall = true,
                        CanLiveTrack = true,
                        CanCancelOrder = false,
                        CanDriverCancelOrder = true,
                        DriverLatitude = Order.DriverId.HasValue == true ? Order.Driver.User.Latitude : null,
                        DriverLongitude = Order.DriverId.HasValue == true ? Order.Driver.User.Longitude : null
                    };
                    if (Order.Status == StoreOrderStatus.Cancelled || Order.Status == StoreOrderStatus.Finished || Order.Status == StoreOrderStatus.Rejected || Order.DriverId.HasValue == false)
                    {
                        chatDTO.CanChat = false;
                        chatDTO.CanCall = false;
                        chatDTO.CanLiveTrack = false;
                    }
                    if (Order.Status == StoreOrderStatus.Placed || Order.Status == StoreOrderStatus.Preparing)
                    {
                        chatDTO.CanCancelOrder = true;
                    }
                    if (Order.Status == StoreOrderStatus.Delivering)
                    {
                        chatDTO.CanFinishOrder = true;
                    }
                    foreach (var msg in Chat)
                    {
                        var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                        var messageDTO = new ChatMessageDTO()
                        {
                            IsMyMessage = msg.FromUserId == CurrentUserId ? true : false,
                            AttachmentFileName = msg.AttachmentName,
                            AttachmentFileType = msg.AttachmentFileType,
                            AttachmentFileUrl = !string.IsNullOrEmpty(msg.AttachmentUrl) ? MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl : null,
                            Date = CreatedOn.ToString("MMM dd, hh:mm tt"),
                            IsAttachment = !string.IsNullOrEmpty(msg.AttachmentUrl) ? true : false,
                            Message = msg.Message,
                            ClientImageUrl = !string.IsNullOrEmpty(Order.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.User.ImageUrl : null,
                            DriverImageUrl = Order.DriverId.HasValue == true ? !string.IsNullOrEmpty(Order.Driver.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.Driver.User.ImageUrl : null : null
                        };
                        chatDTO.Messages.Add(messageDTO);
                    }
                    baseResponse.Data = chatDTO;
                }
            }
            if (OrderType == OrderType.OtlobAy7aga)
            {
                var Order = db.OtlobAy7agaOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && (w.UserId == CurrentUserId || (w.DriverId.HasValue == true && w.Driver.UserId == CurrentUserId)));
                if (Order != null)
                {
                    var Chat = db.OtlobAy7agaOrderChats.Where(x => x.OrderId == OrderId && (x.FromUserId == CurrentUserId || x.ToUserId == CurrentUserId)).OrderBy(d => d.CreatedOn).ToList();
                    ChatDTO chatDTO = new ChatDTO()
                    {
                        IsStoreOrder = false,
                        OrderId = OrderId,
                        ChatId = OrderId,
                        OrderCode = Order.Code,
                        DriverId = Order.DriverId,
                        DriverPhoneNumber = Order.DriverId.HasValue == true ? Order.Driver.User.PhoneNumber : null,
                        ClientPhoneNumber = Order.UserAddressId.HasValue == true ? Order.UserAddress.PhoneNumber : Order.User.PhoneNumber,
                        CanChat = true,
                        CanCall = true,
                        CanLiveTrack = true,
                        CanCancelOrder = false,
                        CanDriverCancelOrder = true,
                        DriverLatitude = Order.DriverId.HasValue == true ? Order.Driver.User.Latitude : null,
                        DriverLongitude = Order.DriverId.HasValue == true ? Order.Driver.User.Longitude : null
                    };
                    if (Order.OrderStatus == OtlobAy7agaOrderStatus.Cancelled || Order.OrderStatus == OtlobAy7agaOrderStatus.Finished || Order.DriverId.HasValue == false)
                    {
                        chatDTO.CanChat = false;
                        chatDTO.CanCall = false;
                        chatDTO.CanLiveTrack = false;
                    }
                    if (Order.OrderStatus == OtlobAy7agaOrderStatus.Placed)
                    {
                        chatDTO.CanCancelOrder = true;
                    }
                    if (Order.OrderStatus == OtlobAy7agaOrderStatus.Started)
                    {
                        chatDTO.CanFinishOrder = true;
                    }
                    foreach (var msg in Chat)
                    {
                        var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                        var messageDTO = new ChatMessageDTO()
                        {
                            IsMyMessage = msg.FromUserId == CurrentUserId ? true : false,
                            IsLocation = false,
                            AttachmentFileName = msg.AttachmentName,
                            AttachmentFileType = msg.AttachmentFileType,
                            AttachmentFileUrl = !string.IsNullOrEmpty(msg.AttachmentUrl) ? MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl : null,
                            Date = CreatedOn.ToString("MMM dd, hh:mm tt"),
                            IsAttachment = !string.IsNullOrEmpty(msg.AttachmentUrl) ? true : false,
                            Message = msg.Message,
                            ClientImageUrl = !string.IsNullOrEmpty(Order.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.User.ImageUrl : null,
                            DriverImageUrl = Order.DriverId.HasValue == true ? !string.IsNullOrEmpty(Order.Driver.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.Driver.User.ImageUrl : null : null
                        };
                        chatDTO.Messages.Add(messageDTO);
                    }
                    baseResponse.Data = chatDTO;
                }
            }


            if (OrderType == OrderType.JobOrder)
            {
                var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && (w.UserId == CurrentUserId || w.Worker.UserId == CurrentUserId));
                if (Order != null)
                {
                    var Chat = db.JobOrderChats.Where(x => x.OrderId == OrderId && (x.FromUserId == CurrentUserId || x.ToUserId == CurrentUserId)).OrderBy(d => d.CreatedOn).ToList();
                    ChatDTO chatDTO = new ChatDTO()
                    {
                        IsStoreOrder = false,
                        OrderId = OrderId,
                        ChatId = OrderId,
                        OrderCode = Order.Code,
                        DriverId = Order.WorkerId,
                        DriverPhoneNumber = Order.Worker.User.PhoneNumber,
                        ClientPhoneNumber = Order.PhoneNumber != null ? Order.PhoneNumber : Order.User.PhoneNumber,
                        CanChat = false,
                        CanCall = false,
                        CanLiveTrack = false,
                        CanCancelOrder = true,
                        CanDriverCancelOrder = false,
                        DriverLatitude = Order.Worker.User.Latitude,
                        DriverLongitude = Order.Worker.User.Longitude
                    };
                    if (Order.Status == JobOrderStatus.AcceptedByWorker)
                    {
                        chatDTO.CanChat = true;
                        chatDTO.CanCall = true;
                        chatDTO.CanLiveTrack = true;
                        chatDTO.CanDriverCancelOrder = true;
                    }
                    if (Order.Status == JobOrderStatus.RejectedByWorker || Order.Status == JobOrderStatus.Finished || Order.Status == JobOrderStatus.CancelledByUser || Order.Status == JobOrderStatus.CancelledByAdmin)
                    {
                        chatDTO.CanCancelOrder = false;
                    }
                    foreach (var msg in Chat)
                    {
                        var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                        var messageDTO = new ChatMessageDTO()
                        {
                            IsMyMessage = msg.FromUserId == CurrentUserId ? true : false,
                            IsLocation = false,
                            AttachmentFileName = msg.AttachmentName,
                            AttachmentFileType = msg.AttachmentFileType,
                            AttachmentFileUrl = !string.IsNullOrEmpty(msg.AttachmentUrl) ? MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl : null,
                            Date = CreatedOn.ToString("MMM dd, hh:mm tt"),
                            IsAttachment = !string.IsNullOrEmpty(msg.AttachmentUrl) ? true : false,
                            Message = msg.Message,
                            ClientImageUrl = !string.IsNullOrEmpty(Order.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.User.ImageUrl : null,
                            DriverImageUrl = !string.IsNullOrEmpty(Order.Worker.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.Worker.User.ImageUrl : null
                        };
                        chatDTO.Messages.Add(messageDTO);
                    }
                    baseResponse.Data = chatDTO;
                }
            }

            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("uploadattachment")]
        public async Task<IHttpActionResult> UploadChatAttachmentFile(UploadChatAttachmentFileDTO fileDTO)
        {
            string CurrentUserId = User.Identity.GetUserId();
            if (fileDTO.OrderType == OrderType.OtlobAy7aga)
            {
                var Order = db.OtlobAy7agaOrders.Find(fileDTO.ChatId);
                if (Order == null && Order.DriverId.HasValue == true)
                {
                    baseResponse.ErrorCode = Errors.OrderNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
                else
                {
                    var Hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    try
                    {
                        var File = Convert.FromBase64String(fileDTO.FileBase64);
                        if (File != null && File.Length > 0)
                        {
                            string ToUserId = null;
                            OtlobAy7agaOrderChat msg = new OtlobAy7agaOrderChat()
                            {
                                FromUserId = CurrentUserId,
                                OrderId = Order.Id,
                                AttachmentFileType = fileDTO.FileType,
                                AttachmentName = fileDTO.FileName,
                                AttachmentUrl = MediaControl.Upload(FilePath.Other, File, fileDTO.FileType)
                            };
                            if (fileDTO.IsDriver == false)
                            {
                                msg.ToUserId = Order.Driver.UserId;
                                ToUserId = Order.Driver.UserId;
                            }
                            else
                            {
                                msg.ToUserId = Order.UserId;
                                ToUserId = Order.UserId;
                            }
                            db.OtlobAy7agaOrderChats.Add(msg);
                            db.SaveChanges();
                            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                            Hub.Clients.Group(Order.Id.ToString()).broadCastAttachment(fileDTO.IsDriver, "", MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl, CreatedOn.ToString("MMM dd, hh:mm tt"));

                            await Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            await Notifications.SendToAllSpecificIOSUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            return Ok(baseResponse);
                        }
                        else
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
            }
            if (fileDTO.OrderType == OrderType.StoreOrder)
            {
                var Order = db.StoreOrders.Find(fileDTO.ChatId);
                if (Order == null && Order.DriverId.HasValue == true)
                {
                    baseResponse.ErrorCode = Errors.OrderNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
                else
                {
                    var Hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    try
                    {
                        var File = Convert.FromBase64String(fileDTO.FileBase64);
                        if (File != null && File.Length > 0)
                        {
                            string ToUserId = null;
                            StoreOrderChat msg = new StoreOrderChat()
                            {
                                FromUserId = CurrentUserId,
                                OrderId = Order.Id,
                                AttachmentFileType = fileDTO.FileType,
                                AttachmentName = fileDTO.FileName,
                                AttachmentUrl = MediaControl.Upload(FilePath.Other, File, fileDTO.FileType)
                            };
                            if (fileDTO.IsDriver == false)
                            {
                                msg.ToUserId = Order.Driver.UserId;
                                ToUserId = Order.Driver.UserId;
                            }
                            else
                            {
                                msg.ToUserId = Order.UserId;
                                ToUserId = Order.UserId;
                            }
                            db.StoreOrderChats.Add(msg);
                            db.SaveChanges();
                            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                            Hub.Clients.Group(Order.Id.ToString()).broadCastAttachment(fileDTO.IsDriver, "", MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl, CreatedOn.ToString("MMM dd, hh:mm tt"));

                            await Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            await Notifications.SendToAllSpecificIOSUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            return Ok(baseResponse);
                        }
                        else
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
            }
            if (fileDTO.OrderType == OrderType.JobOrder)
            {
                var Order = db.JobOrders.Find(fileDTO.ChatId);
                if (Order == null)
                {
                    baseResponse.ErrorCode = Errors.OrderNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
                else
                {
                    var Hub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    try
                    {
                        var File = Convert.FromBase64String(fileDTO.FileBase64);
                        if (File != null && File.Length > 0)
                        {
                            string ToUserId = null;
                            JobOrderChat msg = new JobOrderChat()
                            {
                                FromUserId = CurrentUserId,
                                OrderId = Order.Id,
                                AttachmentFileType = fileDTO.FileType,
                                AttachmentName = fileDTO.FileName,
                                AttachmentUrl = MediaControl.Upload(FilePath.Other, File, fileDTO.FileType)
                            };
                            if (fileDTO.IsDriver == false)
                            {
                                msg.ToUserId = Order.Worker.UserId;
                                ToUserId = Order.Worker.UserId;
                            }
                            else
                            {
                                msg.ToUserId = Order.UserId;
                                ToUserId = Order.UserId;
                            }
                            db.JobOrderChats.Add(msg);
                            db.SaveChanges();
                            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                            Hub.Clients.Group(Order.Id.ToString()).broadCastAttachment(fileDTO.IsDriver, "", MediaControl.GetPath(FilePath.Other) + msg.AttachmentUrl, CreatedOn.ToString("MMM dd, hh:mm tt"));

                            await Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationJobOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            await Notifications.SendToAllSpecificIOSUserDevices(ToUserId, "📎 مرفق", "", NotificationType.ApplicationJobOrderChatPage, Order.Id, !fileDTO.IsDriver, false);
                            return Ok(baseResponse);
                        }
                        else
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
            }
            baseResponse.ErrorCode = Errors.SomethingWentWrong;
            return Content(HttpStatusCode.BadRequest, baseResponse);
        }
    }
}