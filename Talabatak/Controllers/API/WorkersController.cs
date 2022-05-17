using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Talabatak.Models.DTOs;
using Talabatak.Models.Data;
using Talabatak.Models.Enums;
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using Talabatak.Models.ViewModels;
using System.Data.Entity;
using System.Collections.Generic;

namespace Talabatak.Controllers.API
{
    [RoutePrefix("api/workers")]
    public class WorkersController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public WorkersController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("Details")]
        [HttpGet]
        public IHttpActionResult GetWorkerDetails(long WorkerId, string lang = "en")
        {
            var Worker = db.Workers.FirstOrDefault(w => w.IsDeleted == false && w.IsAccepted == true && w.IsBlocked == false && w.Id == WorkerId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.WorkerNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            WorkerProfileDTO profileDTO = new WorkerProfileDTO()
            {
                Name = Worker.User.Name,
                ImageUrl = !string.IsNullOrEmpty(Worker.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Worker.User.ImageUrl : null,
                Rate = Worker.Rate.ToString("N1")
            };

            if (Worker.JobOrders != null)
            {
                foreach (var review in Worker.JobOrders.Where(s => s.IsDeleted == false && s.IsUserReviewed == true))
                {
                    profileDTO.Reviews.Add(new GetWorkerReviewDTO()
                    {
                        ReviewDate = review.UserReviewDate.HasValue == true ? TimeZoneInfo.ConvertTimeFromUtc(review.UserReviewDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time")).ToString("dd MMMM yyyy, hh:mm tt") : null,
                        ReviewDescription = review.UserReviewDescription,
                        ReviewStars = review.UserReviewRate.HasValue == true ? review.UserReviewRate.Value : -1,
                        ReviewerImageUrl = !string.IsNullOrEmpty(review.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + review.User.ImageUrl : null,
                        ReviewerName = review.User.Name
                    });
                }
            }
            baseResponse.Data = profileDTO;
            return Ok(baseResponse);
        }

        [System.Web.Http.Authorize]
        [HttpGet]
        [Route("AcceptOrder")]
        public async Task<IHttpActionResult> AcceptOrder(long OrderId)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(w => w.IsDeleted == false && w.IsAccepted == true && w.UserId == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Worker.IsBlocked == true)
            {
                baseResponse.ErrorCode = Errors.UserIsBlocked;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && w.Status == JobOrderStatus.Placed && w.WorkerId == Worker.Id);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.Status = JobOrderStatus.AcceptedByWorker;
            Order.WorkerProfit = Worker.Profit;
            db.SaveChanges();
            string Title = $"تم قبول طلبك رقم {Order.Code}";
            string Body = $"{Worker.User.Name} قد قبل طلبك رقم {Order.Code}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            return Ok(baseResponse);
        }

        [System.Web.Http.Authorize]
        [Route("RejectOrder")]
        [HttpGet]
        public async Task<IHttpActionResult> RejectOrder(long OrderId)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(w => w.IsDeleted == false && w.IsAccepted == true && w.UserId == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Worker.IsBlocked == true)
            {
                baseResponse.ErrorCode = Errors.UserIsBlocked;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && w.Status == JobOrderStatus.Placed && w.WorkerId == Worker.Id);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.Status = JobOrderStatus.RejectedByWorker;
            db.SaveChanges();
            string Title = $"تم رفض طلبك رقم {Order.Code}";
            string Body = $"{Worker.User.Name} قد رفض طلبك رقم {Order.Code}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            return Ok(baseResponse);
        }

        [System.Web.Http.Authorize]
        [Route("CancelOrder")]
        [HttpGet]
        public async Task<IHttpActionResult> CancelOrder(long OrderId)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(w => w.IsDeleted == false && w.IsAccepted == true && w.UserId == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (Worker.IsBlocked == true)
            {
                baseResponse.ErrorCode = Errors.UserIsBlocked;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && (w.Status == JobOrderStatus.Placed || w.Status == JobOrderStatus.AcceptedByWorker) && w.WorkerId == Worker.Id);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.Status = JobOrderStatus.RejectedByWorker;
            db.SaveChanges();
            string Title = $"تم الغاء طلبك رقم {Order.Code}";
            string Body = $"{Worker.User.Name} قد قام بالغاء طلبك رقم {Order.Code}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
            return Ok(baseResponse);
        }
        [HttpGet]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            var currentUser = User.Identity.GetUserId();
            ReportMobDto model = new ReportMobDto();            
            model.Total = 0;
            var Worker = await db.Workers.Include(x => x.User).Include(x => x.User.City).Include(x => x.User.City.Country).FirstOrDefaultAsync(x => x.UserId == currentUser);

            if (db.JobOrders.Any(x => x.WorkerId == Worker.Id && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))

            {
                model.Total = db.JobOrders.Where(x => x.WorkerId == Worker.Id && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TotalPrice);
            }

            model.Paid = 0;
            if (db.UserWallets.Any(x => x.UserId == Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid += db.UserWallets.Where(x => x.UserId == Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            else if (db.UserWallets.Any(x => x.UserId == Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                      && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                        (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid -= db.UserWallets.Where(x => x.UserId == Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }

            if (db.JobOrders.Any(x => x.WorkerId == Worker.Id
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.JobOrders = (await db.JobOrders.Where(x => x.WorkerId == Worker.Id
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync()).Count;
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.JobOrders.Where(x => (x.WorkerId == Worker.Id) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
                  && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == JobOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.TotalPrice * ((decimal)Order.WorkerProfit / (decimal)100.0);
            }

            model.Stil = model.ForSystem - model.Paid;
            model.Stil = model.ForSystem - model.Paid;
            model.Mobile = Worker.User.PhoneNumber;
            model.Wallet = Worker.User.Wallet;
            model.UserName = Worker.User.Name;
            model.Rate = Worker.Rate;
            baseResponse.Data = model;
            return Ok(baseResponse);
        }
        [System.Web.Http.Authorize]
        [HttpGet]
        [Route("OrderDetails")]
        public IHttpActionResult GetOrderDetails(long OrderId, string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            if (CurrentUserId == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.Unauthorized, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && w.Worker.UserId == CurrentUserId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            UserJobOrderDetailsDTO orderDetailsDTO = new UserJobOrderDetailsDTO()
            {
                Address = Order.Address,
                AddressInDetails = Order.AddressInDetails,
                Apartment = Order.Apartment,
                Building = Order.Building,
                Floor = Order.Floor,
                IsRated = Order.IsUserReviewed,
                Latitude = Order.Latitude,
                Longitude = Order.Longitude,
                OrderDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time")).ToString("dd MMMM yyyy, hh:mm tt"),
                OrderDescription = Order.Description,
                OrderId = OrderId,
                OrderPrice = Order.TotalPrice,
                OrderStatus = Order.Status,
                Rate = Order.UserReviewRate.HasValue == true ? Order.UserReviewRate.Value : -1,
                RateDescription = Order.UserReviewDescription,
                WorkerEmail = Order.Worker.User.Email,
                WorkerName = Order.Worker.User.Name,
                WorkerPhoneNumber = Order.Worker.User.PhoneNumber,
                WorkerRate = Order.Worker.Rate.ToString("N1"),
                ClientEmail = Order.User.Email,
                ClientName = Order.Name == null ? Order.User.Name : Order.Name,
                ClientPhoneNumber = Order.PhoneNumber == null ? Order.User.PhoneNumber : Order.PhoneNumber,
            };
            switch (Order.Status)
            {
                case JobOrderStatus.Placed:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "بانتظار رد العامل" : "New";
                    break;
                case JobOrderStatus.AcceptedByWorker:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "وافق العامل على طلبك" : "Accepted by worker";
                    break;
                case JobOrderStatus.RejectedByWorker:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "رفض العامل طلبك" : "Rejected by worker";
                    break;
                case JobOrderStatus.CancelledByAdmin:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء من الاداره" : "Cancelled by Talabatak";
                    break;
                case JobOrderStatus.CancelledByUser:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                    break;
                case JobOrderStatus.Finished:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الانهاء" : "Finished";
                    break;
                default:
                    break;
            }
            if (Order.Images != null && Order.Images.Count(w => w.IsDeleted == false) > 0)
            {
                foreach (var image in Order.Images.Where(w => w.IsDeleted == false))
                {
                    orderDetailsDTO.OrderImages.Add(MediaControl.GetPath(FilePath.Job) + image.ImageUrl);
                }
            }
            baseResponse.Data = orderDetailsDTO;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("SendFinishCode")]
        public async Task<IHttpActionResult> SendFinishCode(long OrderId, decimal Cost)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(s => s.UserId == CurrentUserId);
            var userWorker = db.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(s => s.Status == JobOrderStatus.AcceptedByWorker && s.IsDeleted == false && s.Id == OrderId && s.Worker.UserId == CurrentUserId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.TotalPrice = Cost;
            db.SaveChanges();
            ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "5ad497d6-1abb-455b-bdcf-3b08ade12f79");
            Admin.Wallet += (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            userWorker.Wallet -= (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            Order.Status = JobOrderStatus.Finished;
            Worker.NumberOfOrders += 1;
            db.SaveChanges();
            string UserTitle = $"تم انهاء طلبكم بنجاح";
            string UserBody = $"سعدنا بخدمتك طلباتك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string WorkerTitle = $"تم انهاء الطلب رقم {Order.Code} بنجاح";
            string WorkerBody = $"لقد قمت بانهاء الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);

            return Ok(baseResponse);
           
        }

        [HttpGet]
        [Route("ResendFinishCode")]
        public async Task<IHttpActionResult> ResendFinishCode(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(s => s.UserId == CurrentUserId);
             var userWorker = db.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(s => s.Status ==  JobOrderStatus.AcceptedByWorker && s.IsDeleted == false && s.Id == OrderId && s.Worker.UserId == CurrentUserId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "5ad497d6-1abb-455b-bdcf-3b08ade12f79");
            Admin.Wallet += (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            userWorker.Wallet -= (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            Order.Status = JobOrderStatus.Finished;
            Worker.NumberOfOrders += 1;
            db.SaveChanges();

            string UserTitle = $"تم انهاء طلبكم بنجاح";
            string UserBody = $"سعدنا بخدمتك طلباتك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string WorkerTitle = $"تم انهاء الطلب رقم {Order.Code} بنجاح";
            string WorkerBody = $"لقد قمت بانهاء الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
     
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Finish")]
        public async Task<IHttpActionResult> FinishOrder(long OrderId, int Code)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(s => s.UserId == CurrentUserId);
            var userWorker = db.Users.FirstOrDefault(x => x.Id == CurrentUserId);
            if (Worker == null)
            {
                baseResponse.ErrorCode = Errors.UserIsNotWorker;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(s => s.Status ==  JobOrderStatus.AcceptedByWorker && s.IsDeleted == false && s.Id == OrderId && s.Worker.UserId == CurrentUserId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.FinishCode != Code)
            {
                baseResponse.ErrorCode = Errors.InvalidFinishCode;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            ApplicationUser Admin = db.Users.FirstOrDefault(x => x.Id == "5ad497d6-1abb-455b-bdcf-3b08ade12f79");
            Admin.Wallet += (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            userWorker.Wallet -= (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0));
            db.SaveChanges();
            db.UserWallets.Add(new UserWallet()
            {
                StoreOrderId = Order.Id,
                TransactionType = TransactionType.SubtractedByAdminManually,
                UserId = CurrentUserId,
                TransactionAmount = (Order.TotalPrice * ((decimal)Order.Worker.Profit / (decimal)100.0))
            });
            db.SaveChanges();
            Order.Status =  JobOrderStatus.Finished;
            Worker.NumberOfOrders += 1;
            db.SaveChanges();

            string UserTitle = $"تم انهاء طلبكم بنجاح";
            string UserBody = $"سعدنا بخدمتك طلباتك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, UserTitle, UserBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, false, true);

            string WorkerTitle = $"تم انهاء الطلب رقم {Order.Code} بنجاح";
            string WorkerBody = $"لقد قمت بانهاء الطلب رقم {Order.Code} بنجاح ، شكراً لك";
            await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, WorkerTitle, WorkerBody, NotificationType.ApplicationOtlobAy7agaOrderDetailsPage, OrderId, true, true);

            return Ok(baseResponse);
        }

    }
}