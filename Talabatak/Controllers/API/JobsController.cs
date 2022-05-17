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
    [AllowAnonymous]
    [RoutePrefix("api/jobs")]
    public class JobsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public JobsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpGet]
        [Route("GetAll")]
        public IHttpActionResult GetAllCategories(string lang = "en")
        {
            var Jobs = db.Jobs.Where(s => s.IsDeleted == false).OrderBy(w => w.SortingNumber).ToList();
            List<JobDTO> jobDTOs = new List<JobDTO>();
            foreach (var job in Jobs)
            {
                jobDTOs.Add(new JobDTO()
                {
                    Id = job.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? job.NameAr : job.NameEn,
                    ImageUrl = !string.IsNullOrEmpty(job.ImageUrl) ? MediaControl.GetPath(FilePath.Job) + job.ImageUrl : null
                });
            }
            baseResponse.Data = jobDTOs;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Workers")]
        public IHttpActionResult GetJobWorkers(long JobId, string lang = "en")
        {
            var Job = db.Jobs.FirstOrDefault(w => w.IsDeleted == false && w.Id == JobId);
            if (Job == null)
            {
                baseResponse.ErrorCode = Errors.JobNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            JobWorkerDTO jobWorkerDTO = new JobWorkerDTO()
            {
                JobName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Job.NameAr : Job.NameEn,
            };

            var Workers = db.JobWorkers.Where(s => s.JobId == JobId && s.IsDeleted == false && s.Worker.IsDeleted == false && s.Worker.IsAccepted == true && s.Worker.IsBlocked == false).ToList();
            if (Workers != null)
            {
                jobWorkerDTO.NumberOfWorkers = Workers.Count();
                foreach (var worker in Workers)
                {
                    jobWorkerDTO.Workers.Add(new WorkerDTO()
                    {
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? worker.Worker.DescriptionAr : worker.Worker.DescriptionEn,
                        JobWorkerId = worker.Id,
                        WorkerId = worker.WorkerId,
                        ImageUrl = !string.IsNullOrEmpty(worker.Worker.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + worker.Worker.User.ImageUrl : null,
                        Name = worker.Worker.User.Name,
                        Rate = worker.Worker.Rate.ToString("N1")
                    });
                }
            }
            baseResponse.Data = jobWorkerDTO;
            return Ok(baseResponse);
        }

        [System.Web.Http.Authorize]
        [HttpGet]
        [Route("BookingDetails")]
        public IHttpActionResult GetBookingPageDetails()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find(CurrentUserId);
            if (user != null)
            {
                baseResponse.Data = new
                {
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                };
                return Ok(baseResponse);
            }
            baseResponse.ErrorCode = Errors.UserNotAuthorized;
            return Content(HttpStatusCode.Unauthorized, baseResponse);
        }

        [System.Web.Http.Authorize]
        [HttpPost]
        [Route("Book")]
        public async Task<IHttpActionResult> BookWorker(BookWorkerDTO model)
        {
            var JobWorker = db.JobWorkers.FirstOrDefault(w => w.IsDeleted == false && w.WorkerId == model.JobWorkerId && w.Worker.IsDeleted == false && w.Worker.IsAccepted == true && w.Worker.IsBlocked == false);
            string CurrentUserId = User.Identity.GetUserId();

            Errors IsValid = ValidateNewBooking(model, JobWorker, CurrentUserId);
            if (IsValid != Errors.Success)
            {
                baseResponse.ErrorCode = IsValid;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            JobOrder order = new JobOrder()
            {
                FinishCode = RandomGenerator.GenerateNumber(1000, 9999),
                Code = RandomGenerator.GenerateNumber(10000, 99999).ToString(),
                Address = model.Address,
                AddressInDetails = model.AddressInDetails,
                Apartment = model.Apartment,
                Building = model.Building,
                Description = model.Description,
                Floor = model.Floor,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Status = JobOrderStatus.Placed,
                UserId = CurrentUserId,
                WorkerId = JobWorker.WorkerId,
                JobId = JobWorker.JobId,
            };

            db.JobOrders.Add(order);

            if (model.Images != null)
            {
                foreach (var image in model.Images.Where(w => w != null))
                {
                    var Image = Convert.FromBase64String(image);
                    db.JobOrderImages.Add(new JobOrderImage()
                    {
                        ImageUrl = MediaControl.Upload(FilePath.Job, Image, MediaType.Image),
                        OrderId = order.Id
                    });
                }
            }

            db.SaveChanges();
            await SMS.SendMessage("", "01024401763", "هناك طلب جديد للصنايعية من فضلك قم بزيارة لوحة التحكم");
            await Notifications.SendToAllSpecificAndroidUserDevices(JobWorker.Worker.UserId, $"طلب جديد فى {JobWorker.Job.NameAr}", $"لديك طلب جديد فى قسم {JobWorker.Job.NameAr}", NotificationType.ApplicationCurrentJobOrders, order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(JobWorker.Worker.UserId, $"طلب جديد فى {JobWorker.Job.NameAr}", $"لديك طلب جديد فى قسم {JobWorker.Job.NameAr}", NotificationType.ApplicationCurrentJobOrders, order.Id, true, true);
            Notifications.SendWebNotification("طلب جديد للصنيعية", "تم ارسال طلبكم الى العامل المختص ، برجاء انتظار رده", "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", order.Id, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);

            await Notifications.SendToAllSpecificAndroidUserDevices(CurrentUserId, $"تم استقبال طلبكم", $"تم ارسال طلبكم الى العامل المختص ، برجاء انتظار رده", NotificationType.ApplicationCurrentJobOrders, order.Id, false, true);
            await Notifications.SendToAllSpecificIOSUserDevices(CurrentUserId, $"تم استقبال طلبكم", $"تم ارسال طلبكم الى العامل المختص ، برجاء انتظار رده", NotificationType.ApplicationCurrentJobOrders, order.Id, false, true);

            var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
            OrdersHub.Clients.Group(JobWorker.WorkerId.ToString()).newOrder();
            return Ok(baseResponse);
        }

        [System.Web.Http.Authorize]
        [HttpGet]
        [Route("UserOrderDetails")]
        public IHttpActionResult UserGetOrderDetails(long OrderId, string lang = "en")
        {
            string CurrentUserId = User.Identity.GetUserId();
            if (CurrentUserId == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.Unauthorized, baseResponse);
            }

            var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && w.UserId == CurrentUserId);
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
            };
            switch (Order.Status)
            {
                case JobOrderStatus.Placed:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "بانتظار رد العامل" : "New";
                    orderDetailsDTO.CanCancel = true;
                    break;
                case JobOrderStatus.AcceptedByWorker:
                    orderDetailsDTO.OrderStatusText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "وافق العامل على طلبك" : "Accepted by worker";
                    orderDetailsDTO.CanCancel = true;
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

        [System.Web.Http.Authorize]
        [HttpPost]
        [Route("RateWorker")]
        public async Task<IHttpActionResult> RateDriver(RateWorkerDTO rateWorkerDTO)
        {
            if (rateWorkerDTO.Stars <= 0 || rateWorkerDTO.Stars > 5)
            {
                baseResponse.ErrorCode = Errors.InvalidStars;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            string CurrentUserId = User.Identity.GetUserId();
            var Order = db.JobOrders.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.Status == JobOrderStatus.Finished && s.Id == rateWorkerDTO.OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Order.UserReviewRate = rateWorkerDTO.Stars;
            Order.UserReviewDescription = rateWorkerDTO.Review;
            Order.IsUserReviewed = true;
            db.SaveChanges();

            Order.Worker.Rate = db.JobOrders.Where(s => s.IsDeleted == false && s.WorkerId == Order.WorkerId && s.IsUserReviewed == true && s.UserReviewRate.HasValue == true).Average(w => w.UserReviewRate.Value);
            db.SaveChanges();
            string Title = $"لديك تقييم جديد";
            string Body = $"قام العميل بتقييمك فى الطلب رقم {Order.Code}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Order.Worker.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Order.Worker.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, Order.Id, true, true);
            return Ok(baseResponse);
        }

        [HttpDelete]
        [Route("UserCancel")]
        public async Task<IHttpActionResult> UserCancelOrder(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.JobOrders.FirstOrDefault(d => d.IsDeleted == false && (d.Status == JobOrderStatus.Placed || d.Status == JobOrderStatus.AcceptedByWorker) && d.UserId == CurrentUserId && d.Id == OrderId);
            if (UserOrder != null)
            {
                UserOrder.Status = JobOrderStatus.CancelledByUser;
                db.SaveChanges();
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                OrdersHub.Clients.Group(UserOrder.WorkerId.ToString()).newOrder(); // To remove the order from these drivers page.
                string Title = $"تم الغاء الطلب رقم {UserOrder.Code}";
                string Body = $"نعتذر ، قام العميل بالغاء طلب العماله رقم {UserOrder.Code}";
                Notifications.SendWebNotification(Title, Body, "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", OrderId, NotificationType.WebOtlobAy7agaOrderDetailsPage, true);
                await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.Worker.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, OrderId, true, true);
                await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.Worker.UserId, Title, Body, NotificationType.ApplicationJobOrderDetails, OrderId, true, true);
            }
            return Ok(baseResponse);
        }


        private Errors ValidateNewBooking(BookWorkerDTO model, JobWorker JobWorker, string CurrentUserId)
        {
            if (string.IsNullOrEmpty(CurrentUserId) || string.IsNullOrWhiteSpace(CurrentUserId))
            {
                return Errors.UserNotAuthorized;
            }

            if (JobWorker == null)
            {
                return Errors.WorkerNotFound;
            }

            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name))
            {
                return Errors.NameIsRequired;
            }

            if (string.IsNullOrEmpty(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                return Errors.PhoneIsRequiredAndInCorrectFormat;
            }

            if (string.IsNullOrEmpty(model.Description) || string.IsNullOrWhiteSpace(model.Description))
            {
                return Errors.OrderDescriptionIsRequired;
            }

            if (model.Images != null)
            {
                foreach (var image in model.Images)
                {
                    try
                    {
                        var Image = Convert.FromBase64String(image);
                        if (Image == null || Image.Length <= 0)
                        {
                            return Errors.FailedToUpload;
                        }
                    }
                    catch (Exception)
                    {
                        return Errors.FailedToUpload;
                    }
                }
            }
            return Errors.Success;
        }
    }
}