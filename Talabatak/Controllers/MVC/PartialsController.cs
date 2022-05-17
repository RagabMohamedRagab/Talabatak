using Talabatak.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Talabatak.Controllers.MVC
{
    public class PartialsController : BaseController
    {
        [Authorize]
        public ActionResult Header()
       {
            var CurrentUser = db.Users.Find(CurrentUserId);
           
            HeaderVM headerVM = new HeaderVM()
            {
                Name = CurrentUser.Name,
                UserImage = CurrentUser.ImageUrl,
            };

            var Notifications = db.Notifications.Where(d => !d.IsDeleted && d.UserId == CurrentUserId).OrderByDescending(d => d.CreatedOn).ToList();
            headerVM.NotificationsNumber = Notifications.Count(d => !d.IsSeen && !d.IsDeleted);
            if (Notifications != null && Notifications.Count > 0)
            {
                foreach (var item in Notifications)
                {
                    headerVM.Notifications.Add(new NotificationVM()
                    {
                        Body = item.Body,
                        NotificationType = item.NotificationType,
                        IsSeen = item.IsSeen,
                        RequestId = item.RequestId,
                        Id = item.Id,
                        Title = item.Title,
                        Link = item.NotificationLink
                    });
                }
            }
            return PartialView(headerVM);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminSideMenu()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            SideMenuVM sideMenuVM = new SideMenuVM()
            {
                UserImage = CurrentUser.ImageUrl,
                Name = CurrentUser.Name,
                Complaints = db.Complaints.Count(w => w.IsDeleted == false && w.IsViewed == false)
            };
            return PartialView(sideMenuVM);
        }

        [Authorize(Roles = "SubAdmin")]
        public ActionResult SubAdminSideMenu()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            SideMenuVM sideMenuVM = new SideMenuVM()
            {
                UserImage = CurrentUser.ImageUrl,
                Name = CurrentUser.Name,
                Complaints = db.Complaints.Count(w => w.IsDeleted == false && w.IsViewed == false)
            };
            return PartialView(sideMenuVM);
        }

        [Authorize(Roles = "Store")]
        public ActionResult StoreSideMenu()
        {
            var CurrentUser = db.Users.Find(CurrentUserId);
            SideMenuVM sideMenuVM = new SideMenuVM()
            {
                UserImage = CurrentUser.ImageUrl,
                Name = CurrentUser.Name
            };
            return PartialView(sideMenuVM);
        }
    }
}