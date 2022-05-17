using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Models.Enums;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class NotificationsController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Users = db.Users.Where(x=> !x.IsDeleted).ToList();
            ViewBag.Drivers = db.Drivers.Where(x=> !x.IsDeleted && ! x.IsBlocked).Select(x => x.User).ToList();
            ViewBag.Workers = db.Workers.Where(x => !x.IsDeleted && !x.IsBlocked).Select(x => x.User).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateNotificationVM notificationVM)
        {
            if (ModelState.IsValid == true)
            {
                if (notificationVM.Users!=null)
                {
                    foreach (var item in notificationVM.Users.ToList())
                    {
                        switch (notificationVM.OS)
                        {
                            case Models.Enums.OS.Android:
                         
                                await Notifications.SendToAllSpecificAndroidUserDevices(item, notificationVM.Title, notificationVM.Body,NotificationType.General,-1, notificationVM.IsDriver,true);
                                break;
                            case Models.Enums.OS.IOS:
                                await Notifications.SendToAllSpecificIOSUserDevices(item, notificationVM.Title, notificationVM.Body, NotificationType.General,-1, notificationVM.IsDriver, true);
                                break;
                            default:
                                await Notifications.SendToAllSpecificAndroidUserDevices(item, notificationVM.Title, notificationVM.Body, NotificationType.General, -1, notificationVM.IsDriver, true);
                                await Notifications.SendToAllSpecificIOSUserDevices(item, notificationVM.Title, notificationVM.Body, NotificationType.General, -1, notificationVM.IsDriver, true);
                                break;
                        }
                    }
                }
                else
                {
                    switch (notificationVM.OS)
                    {
                        case Models.Enums.OS.Android:
                            await Notifications.SendToAllAndroidDevices(notificationVM.Title, notificationVM.Body, notificationVM.IsDriver, notificationVM.IsWorker, true);
                            break;
                        case Models.Enums.OS.IOS:
                            await Notifications.SendToAllIOSDevices(notificationVM.Title, notificationVM.Body, notificationVM.IsDriver, notificationVM.IsWorker, true);
                            break;
                        default:
                            await Notifications.SendToAllAndroidDevices(notificationVM.Title, notificationVM.Body, notificationVM.IsDriver, notificationVM.IsWorker, true);
                            await Notifications.SendToAllIOSDevices(notificationVM.Title, notificationVM.Body, notificationVM.IsDriver, notificationVM.IsWorker, true);
                            break;
                    }
                }
                TempData["SentSuccess"] = true;
                return RedirectToAction("Index");
            }
            TempData["SentError"] = false;
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult MarkNotificationAsSeen(long Id)
        {
            var Notification = db.Notifications.Find(Id);
            if (Notification != null)
            {
                Notification.IsSeen = true;
                CRUD<Models.Domains.Notification>.Update(Notification);
                db.SaveChanges();
            }
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult All(int? Page)
        {
            int pageSize = 40;
            int pageNumber = (Page ?? 1);
            var Notifications = db.Notifications.Where(d => !d.IsDeleted && d.UserId == CurrentUserId).OrderByDescending(w => w.CreatedOn).ToPagedList(pageNumber, pageSize);
            Notifications.ToList().ForEach(w => w.IsSeen = true);
            db.SaveChanges();
            return View(Notifications);
        }
    }
}