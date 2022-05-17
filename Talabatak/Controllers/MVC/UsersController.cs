using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Talabatak.Models.DTOs;
using ClosedXML.Excel;
using System.IO;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        public ActionResult SubAdmins()
        {
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var RoleObject = RoleManager.FindByName("SubAdmin");
            List<ApplicationUser> RoleUsers = new List<ApplicationUser>();
            if (RoleObject != null)
            {
                RoleUsers = db.Users.Where(d => d.Roles.Any(x => x.RoleId == RoleObject.Id)).ToList();
            }
            return View(RoleUsers);
        }

        [HttpGet]
        public ActionResult CreateSubAdmin()
        {
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View();
        }
       
        [HttpPost]
        public async Task<ActionResult> CreateSubAdmin(CreateUserVM model)
        {
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            ValidateUserCreation(model);

            if (ModelState.IsValid == false)
            {
                goto Return;
            }
            else
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string ImageName = null;
                        if (model.Image != null)
                        {
                            ImageName = MediaControl.Upload(FilePath.Users, model.Image);
                        }
                        var user = new ApplicationUser()
                        {
                            PhoneNumberConfirmed = true,
                            EmailConfirmed = true,
                            ImageUrl = ImageName,
                            UserName = model.Email,
                            Email = model.Email,
                            Name = model.Name,
                            PhoneNumber = model.PhoneNumber,
                            VerificationCode = 1111,
                            CityId = model.CityId
                        };
                        IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded == false)
                        {
                            if (model.Image != null)
                            {
                                MediaControl.Delete(FilePath.Users, ImageName);
                            }
                            Transaction.Rollback();
                            ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                            goto Return;
                        }
                        if (UserManager.IsInRole(user.Id, "SubAdmin") == false)
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(user.Id, "SubAdmin");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                                goto Return;
                            }
                        }
                        db.SaveChanges();
                        Transaction.Commit();
                        return RedirectToAction("SubAdmins");
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                        goto Return;
                    }
                }
            }
            Return:
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> EditUser(string Id)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == Id);
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View(new EditUserVM() { 
            CityId=user.CityId,
            Email=user.Email,
            Name=user.Name,
            PhoneNumber=user.PhoneNumber,
            UserId=user.Id,
            ImageUrl=user.ImageUrl
            });
        }
        [HttpPost]
        public async Task<ActionResult> EditUser(EditUserVM model)
        {
            if (!db.Users.Any(x => x.Id == model.UserId))
            {
                return HttpNotFound();
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (ModelState.IsValid == false)
            {
                goto Return;
            }
            else
            {
                    try
                    {
                        string ImageName = null;
                        if (model.Image != null)
                        {
                            ImageName = MediaControl.Upload(FilePath.Users, model.Image);
                            user.ImageUrl = ImageName;
                        }

                        user.PhoneNumberConfirmed = true;
                        user.EmailConfirmed = true;
                        user.UserName = model.Email;
                        user.Email = model.Email;
                        user.Name = model.Name;
                        user.PhoneNumber = model.PhoneNumber;
                        user.VerificationCode = 1111;
                        user.CityId = model.CityId;
                        await db.SaveChangesAsync();
                        
                    }
                    
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                        goto Return;
                    }
            }
            Return:
            ViewBag.Cities = db.Cities.Where(s => s.IsDeleted == false && s.Country.IsDeleted == false).ToList();
            return View(model);
        }
        [HttpGet]
        public ActionResult ToggleDelete(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                if (user.IsDeleted == true)
                {
                    user.IsDeleted = false;
                }
                else
                {
                    user.IsDeleted = true;
                }
                db.SaveChanges();
            }
            return RedirectToAction("SubAdmins");
        }

        [HttpGet]
        public ActionResult ChangePassword(string Id)
        {
            if (string.IsNullOrEmpty(Id) == false)
            {
                var user = db.Users.Find(Id);
                if (user != null)
                {
                    ViewBag.UserId = Id;
                    return View();
                }
            }
            return RedirectToAction("SubAdmins");
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (ModelState.ContainsKey("CurrentPassword"))
            {
                ModelState["CurrentPassword"].Errors.Clear();
            }
            if (ModelState.IsValid == true)
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var user = db.Users.Find(model.UserId);
                if (user != null)
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    return RedirectToAction("SubAdmins");
                }
            }
            ViewBag.UserId = model.UserId;
            return View(model);
        }

        private void ValidateUserCreation(CreateUserVM model)
        {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", "اسم المستخدم مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (string.IsNullOrEmpty(model.PhoneNumber))
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                }

                if (string.IsNullOrEmpty(model.Password))
                    ModelState.AddModelError("Password", "كلمة السر مطلوبة");

                if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 6)
                    ModelState.AddModelError("Password", "كلمة السر يجب ان لا تقل عن 6 أحرف");

                if (string.IsNullOrEmpty(model.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "تأكيد كلمة السر مطلوبة");

                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "كلمة السر غير متطابقة");

                if (string.IsNullOrEmpty(model.Email))
                    ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");

                if (model.Image != null)
                {
                    bool IsImageValid = CheckFiles.IsImage(model.Image);
                    if (IsImageValid == false)
                        ModelState.AddModelError("Image", "صوره المسؤول غير صحيحة");
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var IsEmailExist = db.Users.Any(d => d.Email.ToLower() == model.Email.ToLower());
                    if (IsEmailExist == true)
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                    }
                }

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var IsPhoneExist = db.Users.Any(d => d.PhoneNumber.ToLower() == model.PhoneNumber.ToLower());
                    if (IsPhoneExist == true)
                    {
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                    }
                }
        }
        [HttpGet]
        public ActionResult Clients()
        {
            RoleManager<IdentityRole> RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            IdentityRole adminRole = RoleManager.FindByName("Admin");
            IdentityRole driverRole = RoleManager.FindByName("Driver");
            IdentityRole workerRole = RoleManager.FindByName("Worker");
            List<ApplicationUser> Users = db.Users.Include(u=>u.Roles)
                .Where(u => 
                !u.Roles.Select(r=>r.RoleId).Contains(adminRole.Id) &&
                !u.Roles.Select(r=>r.RoleId).Contains(driverRole.Id) &&
                !u.Roles.Select(r=>r.RoleId).Contains(workerRole.Id)).ToList();
            Users = (Users.Except(db.StoreUsers.Include(x => x.User).Select(x => x.User).Distinct().ToList())).ToList();
            TempData["Users"] = Users;
            return View(Users);
        }
        public async Task<ActionResult> DownloadExecl()
        {
            var Users = TempData["Users"] as List<ApplicationUser>;
            var dt = ExcelExport.UsersExport(Users);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
                }
            }
        }

        [HttpGet]
        public ActionResult Details(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                return View(user);
            }
            return RedirectToAction("Clients");
        }
        public async Task<ActionResult> Report(string UserId, DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return View(nameof(Clients));
            }
           
            if (!await db.Users.AnyAsync(x => x.Id ==UserId))
            {
                return View(nameof(Clients));
            }
            var model = new UserReportDto()
            {
                User = db.Users.Include(x=>x.City).Include(x=>x.City.Country).FirstOrDefault(x => x.Id == UserId),

                StoreOrders = db.StoreOrders.Where(x => x.UserId == UserId && (!x.IsDeleted) && 
                (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToList(),

                JobOrders = db.JobOrders.Where(x => x.UserId == UserId && (!x.IsDeleted) && 
                (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToList(),

                OtlobAy7AgaOrders = db.OtlobAy7agaOrders.Where(x => x.UserId == UserId && (!x.IsDeleted) && 
                (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToList()
            };
            return View(model);
        }
        public ActionResult Verify(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                user.PhoneNumberConfirmed = true;
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Clients");
        }

        [HttpGet]
        public ActionResult ToggleBlock(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                if (user.IsDeleted == true)
                {
                    user.IsDeleted = false;
                }
                else
                {
                    user.IsDeleted = true;
                }
                db.SaveChanges();
                TempData["Success"] = true;
            }
            return RedirectToAction("Clients");
        }

        [HttpGet]
        public ActionResult Wallet(string Id)
        {
            var user = db.Users.Find(Id);
            if (user != null)
            {
                return View(user);
            }
            return RedirectToAction("Clients");
        }

        [HttpPost]
        public ActionResult AddOrSubtractToUserWallet(string UserId, decimal Amount, string Way, HttpPostedFileBase Attachment, bool IsAdd)
        {
            var user = db.Users.Find(UserId);
            if (user == null)
            {
                TempData["SubmitError"] = "العضو المطلوب غير متاح";
                return RedirectToAction("Wallet", new { Id = UserId });
            }
            if (Amount <= 0)
            {
                TempData["SubmitError"] = "المبلغ المطلوب اضافته غير صحيح";
                return RedirectToAction("Wallet", new { Id = UserId });
            }
            if (Attachment != null)
            {
                if (CheckFiles.IsValidFile(Attachment) == false)
                {
                    TempData["SubmitError"] = "البيان المرفق ملف غير صحيح";
                    return RedirectToAction("Wallet", new { Id = UserId });
                }
            }
            if (IsAdd == true)
            {
                user.Wallet += Amount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Amount,
                    TransactionType = TransactionType.AddedByAdminManually,
                    UserId = UserId,
                    TransactionWay = Way,
                };
                if (Attachment != null)
                {
                    userWallet.AttachmentUrl = MediaControl.Upload(FilePath.Other, Attachment);
                }
                db.UserWallets.Add(userWallet);
            }
            else
            {
                user.Wallet -= Amount;
                UserWallet userWallet = new UserWallet()
                {
                    TransactionAmount = Amount,
                    TransactionType = TransactionType.SubtractedByAdminManually,
                    UserId = UserId,
                    TransactionWay = Way,
                };
                if (Attachment != null)
                {
                    userWallet.AttachmentUrl = MediaControl.Upload(FilePath.Other, Attachment);
                }
                db.UserWallets.Add(userWallet);
            }
            db.SaveChanges();
            TempData["Success"] = true;
            TempData["SubmitSuccess"] = true;
            return RedirectToAction("Wallet", new { Id = UserId });
        }
    }
}