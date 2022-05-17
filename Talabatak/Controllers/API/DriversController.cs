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
    [System.Web.Http.Authorize]
    [RoutePrefix("api/Drivers")]
    public class DriversController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private ApplicationUserManager _userManager;

        public DriversController()
        {
            baseResponse = new BaseResponseDTO();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public IHttpActionResult Login(string PhoneNumberWithoutKey, string Password)
        {
            if (string.IsNullOrEmpty(PhoneNumberWithoutKey))
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.PhoneIsRequiredAndInCorrectFormat });

            if (string.IsNullOrEmpty(Password))
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.PasswordIsRequiredOrMinimumLength });

            var CurrentUser = db.Users.FirstOrDefault(x => x.PhoneNumber == PhoneNumberWithoutKey);
            if (CurrentUser == null)
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.UserNotFound });

            var result = UserManager.PasswordHasher.VerifyHashedPassword(CurrentUser.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.WrongPassword });

            var DeliveryMan = db.Drivers.FirstOrDefault(x => x.UserId == CurrentUser.Id && x.IsDeleted == false);
            var Worker = db.Workers.FirstOrDefault(x => x.UserId == CurrentUser.Id && x.IsDeleted == false);
            if (DeliveryMan == null && Worker == null)
            {
                return Content(HttpStatusCode.NotFound, new { ErrorCode = Errors.UserNotFound });
            }

            if ((DeliveryMan != null && DeliveryMan.IsBlocked == true) || (Worker != null && Worker.IsBlocked == true))
            {
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.UserIsBlocked });
            }
            if (DeliveryMan != null)
            {
                var manDTO = DriverDTO.toDeliveryManDTO(DeliveryMan);
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string uri = new Uri(Request.RequestUri.ToString()).ToString();
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var baseUrl = Request.RequestUri.AbsoluteUri.Split(new[] { Request.RequestUri.AbsolutePath }, StringSplitOptions.None)[0];
                    var toke = webClient.UploadString($"{baseUrl}/Token", "POST", $"grant_type=password&username={CurrentUser.Email}&password={Password}");
                    var jObject = JObject.Parse(toke);
                    manDTO.Token = jObject.GetValue("access_token").ToString();
                }
                catch (Exception)
                {
                }
                DeliveryMan.IsOnline = true;
                db.SaveChanges();
                return Ok(new { Worker = manDTO });
            }
            else
            {
                var manDTO = WorkerDTO.toDeliveryManDTO(Worker);
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string uri = new Uri(Request.RequestUri.ToString()).ToString();
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var baseUrl = Request.RequestUri.AbsoluteUri.Split(new[] { Request.RequestUri.AbsolutePath }, StringSplitOptions.None)[0];
                    var toke = webClient.UploadString($"{baseUrl}/Token", "POST", $"grant_type=password&username={CurrentUser.Email}&password={Password}");
                    var jObject = JObject.Parse(toke);
                    manDTO.Token = jObject.GetValue("access_token").ToString();
                }
                catch (Exception)
                {
                }
                db.SaveChanges();
                return Ok(new { Worker = manDTO });
            }
        }
        [HttpPost]
        [Route("ActiveToggle")]
        public IHttpActionResult ActiveToggle()
        {
            var CurrentUserId = User.Identity.GetUserId();
            var user = db.Drivers.FirstOrDefault(x => x.UserId == CurrentUserId);
            user.IsOnline = !user.IsOnline;
            db.SaveChanges();
            return Ok(baseResponse);
        }
        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout(PushTokenDTO logOutDTO)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var UserToken = db.PushTokens.FirstOrDefault(x => x.IsDeleted == false && x.UserId == CurrentUserId && x.OS == logOutDTO.OS && x.Token == logOutDTO.PushToken && x.IsWorker == true);
            if (UserToken != null)
            {
                CRUD<PushToken>.Delete(UserToken);
                db.SaveChanges();
            }
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver != null)
            {
                Driver.IsOnline = false;
                db.SaveChanges();
            }
            return Ok(baseResponse);
        }
        [HttpGet]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            string CurrentUserId = User.Identity.GetUserId();
            ReportMobDto model = new ReportMobDto();          
            model.Total = 0;
            var Driver = await db.Drivers.Include(x => x.User).Include(x => x.User.City).Include(x => x.User.City.Country).FirstOrDefaultAsync(x => x.UserId == CurrentUserId);

            if (db.OtlobAy7agaOrders.Any(x => x.DriverId == Driver.Id && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Total = db.OtlobAy7agaOrders.Where(x => x.DriverId == Driver.Id && x.OrderStatus == OtlobAy7agaOrderStatus.Finished &&
               (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.DeliveryFees);
            }
            if (db.StoreOrders.Any(x => x.DriverId == Driver.Id && x.Status == StoreOrderStatus.Finished
            && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Total += db.StoreOrders.Where(x => x.DriverId == Driver.Id && x.Status == StoreOrderStatus.Finished
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.DeliveryFees);
            }
            model.Paid = 0;
            if (db.UserWallets.Any(x => x.UserId == Driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid += db.UserWallets.Where(x => x.UserId == Driver.UserId && x.TransactionType == TransactionType.AddedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            else if (db.UserWallets.Any(x => x.UserId == Driver.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                      && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                        (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid -= db.UserWallets.Where(x => x.UserId == Driver.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            if (db.StoreOrders.Any(x => x.DriverId == Driver.Id
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.StoreOrders = (await db.StoreOrders.Where(x => x.DriverId == Driver.Id
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync()).Count;
            }
    
            if (db.OtlobAy7agaOrders.Any(x => x.DriverId == Driver.Id
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.OtlobAy7AgaOrders = (await db.OtlobAy7agaOrders.Where(x => x.DriverId == Driver.Id
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync()).Count;
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.StoreOrders.Where(x => (x.DriverId == Driver.Id) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == StoreOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            foreach (var Order in db.OtlobAy7agaOrders.Where(x => (x.DriverId == Driver.Id) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
           && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.OrderStatus == OtlobAy7agaOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.DeliveryFees * ((decimal)Order.DeliveryProfit / (decimal)100.0);
            }
            model.Stil = model.ForSystem - model.Paid;
            model.Mobile = Driver.User.PhoneNumber;
            model.Wallet = Driver.User.Wallet;
            model.UserName = Driver.User.Name;
            model.Rate = Driver.Rate;
            model.driverReviews = (db.DriverReviews.Include(x => x.User).Where(x => x.DriverId == Driver.Id).ToList()).Count;
            baseResponse.Data = model;
            return Ok(baseResponse);
        }
        [Route("GetProfile")]
        public IHttpActionResult GetProfile()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(w => w.UserId == CurrentUserId);
            var Worker = db.Workers.FirstOrDefault(w => w.UserId == CurrentUserId);
            if (Driver != null)
            {
                return Ok(new { Worker = DriverDTO.toDeliveryManDTO(Driver) });
            }
            if (Worker != null)
            {
                return Ok(new { Worker = DriverDTO.toDeliveryManDTO(Driver) });
            }
            baseResponse.ErrorCode = Errors.UserIsNotWorker;
            return Content(HttpStatusCode.BadRequest, baseResponse);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(string Phone)
        {
            if (string.IsNullOrEmpty(Phone) || string.IsNullOrWhiteSpace(Phone))
            {
                baseResponse.ErrorCode = Errors.PhoneIsRequiredAndInCorrectFormat;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var CurrentUser = db.Users.FirstOrDefault(x => x.PhoneNumber == Phone);
            if (CurrentUser == null)
            {
                baseResponse.ErrorCode = Errors.UserNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            int newPass = RandomGenerator.GenerateNumber(100000, 999999);
            string hashedPass = UserManager.PasswordHasher.HashPassword(newPass.ToString());
            CurrentUser.PasswordHash = hashedPass;
            db.SaveChanges();
            await SMS.SendMessage("249", CurrentUser.PhoneNumber, $"كلمه السر الجديده لحسابك هى {newPass}");
            return Ok();
        }

        [HttpPost]
        [Route("AddComplaint")]
        public IHttpActionResult AddComplaint(ComplaintDTO complaintDTO)
        {
            if (!ModelState.IsValid)
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = Errors.MessageIsRequired });

            string CurrentUserId = User.Identity.GetUserId();
            Complaint complaint = new Complaint()
            {
                Message = complaintDTO.ComplaintReport,
                UserId = CurrentUserId
            };
            db.Complaints.Add(complaint);
            db.SaveChanges();
            return Ok(new { ErrorCode = Errors.Success });
        }

        [HttpGet]
        [Route("GetCurrentStatus")]
        public IHttpActionResult GetCurrentStatus()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(x => x.UserId == CurrentUserId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.UserNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            baseResponse.Data = new { IsAvailable = Driver.IsOnline };
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("ToggleSwitch")]
        public IHttpActionResult ToggleSwitch()
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(x => x.UserId == CurrentUserId);
            if (Driver != null)
            {
                if (Driver.IsOnline)
                {
                    Driver.IsOnline = false;
                }
                else
                {
                    Driver.IsOnline = true;
                }
            }
            db.SaveChanges();
            return Ok(baseResponse);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}