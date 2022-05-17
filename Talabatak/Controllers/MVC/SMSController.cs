using Talabatak.Helpers;
using Talabatak.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class SMSController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Users = db.Users.Where(s => s.IsDeleted == false).ToList();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(SendSMSVM model)
        {
            if (model.IsAllUsers == false && (model.Users == null || model.Users.Any() == false) && (string.IsNullOrEmpty(model.PhoneNumbers) || string.IsNullOrWhiteSpace(model.PhoneNumbers)))
            {
                ModelState.AddModelError("Users", "برجاء اختيار المرسل اليهم");
            }
            if (ModelState.IsValid == true)
            {
                string Result = null;
                if (string.IsNullOrEmpty(model.PhoneNumbers) == false && string.IsNullOrWhiteSpace(model.PhoneNumbers) == false)
                {
                    List<string> PhoneNumbers = new List<string>();
                    try
                    {
                        PhoneNumbers = model.PhoneNumbers.Split(',').ToList();
                        if (PhoneNumbers != null && PhoneNumbers.Count() > 0)
                        {
                            Result = await SMS.SendMessageToMultipleUsers("249", PhoneNumbers, model.Message);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (model.IsAllUsers == false && model.Users != null)
                {
                    if (model.Users.Count() == 1)
                    {
                        var user = db.Users.Find(model.Users.FirstOrDefault());
                        if (user != null)
                        {
                            Result = await SMS.SendMessage("249", user.PhoneNumber, model.Message);
                        }
                    }
                    else
                    {
                        var users = db.Users.Where(w => model.Users.Contains(w.Id)).ToList();
                        if (users != null && users.Count() > 0)
                        {
                            Result = await SMS.SendMessageToMultipleUsers("249", users.Select(x => x.PhoneNumber).ToList(), model.Message);
                        }
                    }
                }
                else if (model.IsAllUsers == true)
                {
                    var users = db.Users.Where(s => s.IsDeleted == false).ToList();
                    if (users != null && users.Count() > 0)
                    {
                        Result = await SMS.SendMessageToMultipleUsers("249", users.Select(x => x.PhoneNumber).ToList(), model.Message);
                    }
                }
                if (Result == "تم ارسال الرسالة بنجاح")
                {
                    TempData["SentSuccess"] = true;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["SentError"] = Result;
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Users = db.Users.Where(s => s.IsDeleted == false).ToList();
            return View();
        }
    }
}