using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
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

namespace Talabatak.Web.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/PushTokens")]
    public class PushTokensController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public PushTokensController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("Add")]
        [HttpPost]
        public IHttpActionResult AddUserToken(PostPushTokenDTO tokenDTO)
        {
            if (tokenDTO.OS != OS.Android && tokenDTO.OS != OS.IOS)
            {
                baseResponse.ErrorCode = Errors.InvalidOperatingSystem;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (string.IsNullOrEmpty(tokenDTO.PushToken))
            {
                baseResponse.ErrorCode = Errors.InvalidPushToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            string CurrentUserId = User.Identity.GetUserId();
            var PushTokenInDB = db.PushTokens.FirstOrDefault(x => x.IsWorker == tokenDTO.IsDriver && x.Token == tokenDTO.PushToken && x.OS == tokenDTO.OS && x.UserId == CurrentUserId);
            if (PushTokenInDB != null)
            {
                if (PushTokenInDB.IsDeleted == true)
                    CRUD<PushToken>.Restore(PushTokenInDB);
            }
            else
            {
                PushToken pushToken = new PushToken()
                {
                    Token = tokenDTO.PushToken,
                    OS = tokenDTO.OS,
                    UserId = CurrentUserId,
                    IsWorker = tokenDTO.IsDriver
                };
                db.PushTokens.Add(pushToken);
            }
            db.SaveChanges();
            baseResponse.Data = new { NotificationsCount = db.Notifications.Count(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == CurrentUserId).ToString() };
            return Ok(baseResponse);
        }

        [Route("get")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetUserNotifications(bool IsWorker = false)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var Notifications = db.Notifications.Where(d => d.IsDeleted == false && d.UserId == CurrentUserId && d.IsWorker == IsWorker).OrderByDescending(d => d.CreatedOn).ThenByDescending(d => d.IsSeen).ToList();
            if (Notifications == null || Notifications.Count() <= 0)
            {
                return Ok(baseResponse);
            }
            List<NotificationDTO> notificationDTOs = new List<NotificationDTO>();
            foreach (var item in Notifications)
            {
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(item.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                var notify = db.Notifications.FirstOrDefault(x => x.Id == item.Id);
                notify.IsSeen = true;
                db.SaveChanges();
                notificationDTOs.Add(new NotificationDTO()
                {
                    Description = item.Body,
                    IsSeen = item.IsSeen,
                    NotificationId = item.Id,
                    Time = CreatedOn.ToString("dd MMM, hh:mm tt"),
                    Title = item.Title,
                    Type = item.NotificationType,
                    RequestId = item.RequestId
                });
            }
          
            baseResponse.Data = notificationDTOs;
            return Ok(baseResponse);
        }
    }
}
