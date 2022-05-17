using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace Talabatak.Controllers.MVC
{
    [Authorize(Roles = "Admin, SubAdmin")]
    public class AppJobsController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string q)
        {
            if (!string.IsNullOrEmpty(q) && q.ToLower() == "deleted")
            {
                ViewBag.Jobs = db.Jobs.Where(x => x.IsDeleted).ToList();
            }
            else
            {
                ViewBag.Jobs = db.Jobs.Where(x => !x.IsDeleted).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(Job job, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                var LatestSortingNumber = db.Jobs.Select(s => s.SortingNumber).DefaultIfEmpty(0).Max();
                job.SortingNumber = LatestSortingNumber + 1;
                if (Image != null)
                {
                    job.ImageUrl = MediaControl.Upload(FilePath.Job, Image);
                }
                db.Jobs.Add(job);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Jobs = db.Jobs.Where(x => !x.IsDeleted).ToList();
            return View(job);
        }
        public async Task<ActionResult> Finshed(long? OrderId)
        {
            if (await db.JobOrders.AnyAsync(x => x.Id == OrderId))
            {
                var FinshedOrder = await db.JobOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                FinshedOrder.Status = JobOrderStatus.Finished;
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Job job = db.Jobs.Find(id);
            if (job == null)
                return RedirectToAction("Index");
            return View(job);
        }

        [HttpPost]
        public ActionResult Edit(Job job, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                bool IsImage = CheckFiles.IsImage(Image);
                if (!IsImage)
                {
                    ModelState.AddModelError("", "الصوره غير صحيحة");
                }
            }
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    if (job.ImageUrl != null)
                    {
                        MediaControl.Delete(FilePath.Job, job.ImageUrl);
                    }
                    job.ImageUrl = MediaControl.Upload(FilePath.Job, Image);
                }
                db.Entry(job).State = EntityState.Modified;
                CRUD<Job>.Update(job);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(job);
        }

        [HttpGet]
        public ActionResult ToggleDeleteJob(long? JobId)
        {
            if (JobId.HasValue == false)
                return RedirectToAction("Index");

            var Job = db.Jobs.Find(JobId);
            if (Job != null)
            {
                if (Job.IsDeleted == false)
                {
                    CRUD<Job>.Delete(Job);
                }
                else
                {
                    CRUD<Job>.Restore(Job);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult SetSortingNumber(long JobId, int Number)
        {
            var Job = db.Jobs.Find(JobId);
            if (Job == null)
            {
                return Json(new { Sucess = false, Message = "المهنة المطلوبة غير متوفر" }, JsonRequestBehavior.AllowGet);
            }

            var ExistingJobNumber = db.Jobs.FirstOrDefault(w => w.Id != JobId && w.SortingNumber == Number);
            if (ExistingJobNumber != null)
            {
                return Json(new { Sucess = false, Message = $"المهنة {ExistingJobNumber.NameAr} لها نفس الترتيب" }, JsonRequestBehavior.AllowGet);
            }

            Job.SortingNumber = Number;
            CRUD<Job>.Update(Job);
            db.SaveChanges();
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Workers(long? JobId)
        {
            if (JobId.HasValue == true)
            {
                var Job = db.Jobs.Find(JobId);
                if (Job == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Job = Job;
                    return View(db.JobWorkers.Where(s => s.JobId == JobId).OrderBy(w => w.Worker.User.Name).Select(w => w.Worker).ToList());
                }
            }
            else
            {
                return View(db.Workers.OrderBy(w => w.User.Name).ToList());
            }
        }
        public async Task<ActionResult> Report(long? WorkerId, DateTime? FilterTime, DateTime? FilterTimeTo)
        {
            DriverReportVM model = new DriverReportVM();
            if (!WorkerId.HasValue)
            {
                return View(nameof(Index));
            }
            if (WorkerId.HasValue == true)
            {
                if (!await db.Workers.AnyAsync(x => x.Id == WorkerId.Value))
                {
                    return View(nameof(Index));
                }
            }
            model.Total = 0;
            model.Worker = await db.Workers.Include(x => x.User).Include(x => x.User.City).Include(x => x.User.City.Country).FirstOrDefaultAsync(x => x.Id == WorkerId.Value);

            if (db.JobOrders.Any(x => x.WorkerId == WorkerId && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))

            {
                model.Total = db.JobOrders.Where(x => x.WorkerId == WorkerId && x.Status == JobOrderStatus.Finished &&
           (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
            (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x =>x.TotalPrice);
            }
            
            model.Paid = 0;
            if (db.UserWallets.Any(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                 (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid += db.UserWallets.Where(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.AddedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }
            else if (db.UserWallets.Any(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                      && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                        (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.Paid -= db.UserWallets.Where(x => x.UserId == model.Worker.UserId && x.TransactionType == TransactionType.SubtractedByAdminManually
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
               (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true))
                    .Sum(x => x.TransactionAmount);
            }

            model.JobOrders = new List<JobOrder>();
            if (db.JobOrders.Any(x => x.WorkerId == WorkerId
             && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)))
            {
                model.JobOrders = await db.JobOrders.Where(x => x.WorkerId == WorkerId
                && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true) &&
                (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true)).ToListAsync();
            }
            model.ForSystem = (decimal)0.0;
            foreach (var Order in db.JobOrders.Where(x => (x.WorkerId==WorkerId) && (FilterTime.HasValue ? DateTime.Compare(FilterTime.Value, x.CreatedOn) <= 0 : true)
                  && (FilterTimeTo.HasValue ? DateTime.Compare(FilterTimeTo.Value, x.CreatedOn) >= 0 : true) && x.Status == JobOrderStatus.Finished).ToList())
            {
                model.ForSystem += Order.TotalPrice * ((decimal)Order.WorkerProfit / (decimal)100.0);
            }

            model.Stil = model.ForSystem - model.Paid;
            model.Done = model.JobOrders.Where(x => x.Status == JobOrderStatus.Finished).Count();
            model.Cancel = (model.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByUser).Count())
                +(model.JobOrders.Where(x => x.Status == JobOrderStatus.CancelledByAdmin).Count());
            model.Reject = model.JobOrders.Where(x => x.Status == JobOrderStatus.RejectedByWorker).Count();
            ViewBag.Title = $"تقرير العامــل {model.Worker.User.Name}";
            return View(model);
        }
        [HttpGet]
        public ActionResult CreateWorker()
        {
            ViewBag.Jobs = db.Jobs.Where(s => s.IsDeleted == false).ToList();
            ViewBag.Users = db.Users.Where(d => d.Workers.Any(s => s.UserId == d.Id) == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Join()
        {
            ViewBag.Jobs = db.Jobs.Where(s => s.IsDeleted == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Join(CreateWorkerVM model)
        {
            return await RegisterTheWorker(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Success()
        {
            if (TempData["RegisterSuccess"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Join");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateWorker(CreateWorkerVM model)
        {
            return await RegisterTheWorker(model);
        }

        private async Task<ActionResult> RegisterTheWorker(CreateWorkerVM model)
        {
            var Validation = ValidateWorkerCreation(model, ModelState);
            if (Validation.IsValid == false)
            {
                goto Jump;
            }

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            bool isValidGuid = false;
            try
            {
                Guid guid = new Guid();
                isValidGuid = Guid.TryParse(model.UserId, out guid);
            }
            catch (Exception)
            {
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    string UserId = null;
                    if (isValidGuid == true)
                    {
                        UserId = model.UserId;
                        if (UserManager.IsInRole(model.UserId, "Worker") == false)
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(model.UserId, "Worker");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                                goto Jump;
                            }
                        }
                    }
                    else
                    {
                        var City = db.Cities.Find(model.CityId);
                        var user = new ApplicationUser()
                        {
                            CreatedOn = DateTime.Now.ToUniversalTime(),
                            PhoneNumberConfirmed = true,
                            EmailConfirmed = true,
                            ImageUrl = MediaControl.Upload(FilePath.Users, model.PersonalPhoto),
                            UserName = model.Email,
                            Email = model.Email,
                            CityId = model.CityId,
                            Name = model.Name,
                            RegisterationType = RegisterationType.Internal,
                            PhoneNumber = model.PhoneNumber,
                            VerificationCode = 1111,
                        };
                        IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded == false)
                        {
                            Transaction.Rollback();
                            ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                            goto Jump;
                        }
                        else
                        {
                            IdentityResult RoleResult = UserManager.AddToRole(user.Id, "Worker");
                            if (RoleResult.Succeeded == false)
                            {
                                Transaction.Rollback();
                                ModelState.AddModelError("", "شئ ما خطأ ، برجاء المحاوله لاحقاً");
                                goto Jump;
                            }
                        }
                        UserId = user.Id;
                    }

                    Worker worker = new Worker()
                    {
                        IsBlocked = false,
                        UserId = UserId,
                        NumberOfOrders = 0,
                        Rate = 0,
                        DescriptionAr = model.DescriptionAr,
                        DescriptionEn = model.DescriptionEn,
                        IdentityPhoto = MediaControl.Upload(FilePath.Users, model.IdentityPhoto),
                        Profit = model.Profit
                    };
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        worker.IsAccepted = true;
                        worker.AcceptedOn = DateTime.Now.ToUniversalTime();
                    }
                    db.Workers.Add(worker);

                    if (model.JobIds != null)
                    {
                        foreach (var job in model.JobIds)
                        {
                            db.JobWorkers.Add(new JobWorker()
                            {
                                JobId = job,
                                WorkerId = worker.Id,
                            });
                        }
                    }

                    db.SaveChanges();
                    Transaction.Commit();
                    if (User.IsInRole("Admin") || User.IsInRole("SubAdmin"))
                    {
                        TempData["Success"] = true;
                        return RedirectToAction("Workers");
                    }
                    else
                    {
                        Notifications.SendWebNotification($"العامل {model.Name} يريد الانضمام", $"العامل {model.Name} يريد الانضمام", Role: "Admin", Id: worker.Id, notificationLinkType: NotificationType.AdminWorkersPage, IsSaveInDatabase: true);
                        TempData["RegisterSuccess"] = true;
                        return RedirectToAction("Success");
                    }
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    goto Jump;
                }
            }

            Jump:
            ViewBag.Jobs = db.Jobs.Where(s => s.IsDeleted == false).ToList();
            ViewBag.Users = db.Users.Where(d => d.Workers.Any(s => s.UserId == d.Id) == false).ToList();
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        private ModelStateDictionary ValidateWorkerCreation(CreateWorkerVM model, ModelStateDictionary ModelState)
        {
            if (model.PersonalPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.PersonalPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("PersonalPhoto", "صوره العامل الشخصية غير صحيحة");
            }

            if (model.JobIds == null || model.JobIds.Count() <= 0)
            {
                ModelState.AddModelError("JobIds", "وظائف العامل مطلوبة");
            }
            
            bool isValidGuid = false;
            try
            {
                Guid guid = new Guid();
                isValidGuid = Guid.TryParse(model.UserId, out guid);
            }
            catch (Exception)
            {
            }

            if (isValidGuid == true)
            {
                var user = db.Users.Find(model.UserId);
                if (user == null)
                {
                    ModelState.AddModelError("UserId", "العضو المطلوب غير متاح");
                }
                else
                {
                    var Worker = db.Workers.FirstOrDefault(d => d.UserId == user.Id);
                    if (Worker != null)
                    {
                        ModelState.AddModelError("UserId", "هذا العضو مسجل كسائق من قبل");
                    }
                }
                if (ModelState.ContainsKey("Name"))
                    ModelState["Name"].Errors.Clear();

                if (ModelState.ContainsKey("Email"))
                    ModelState["Email"].Errors.Clear();

                if (ModelState.ContainsKey("PhoneNumber"))
                    ModelState["PhoneNumber"].Errors.Clear();

                if (ModelState.ContainsKey("CityId"))
                    ModelState["CityId"].Errors.Clear();

                if (ModelState.ContainsKey("Password"))
                    ModelState["Password"].Errors.Clear();

                if (ModelState.ContainsKey("ConfirmPassword"))
                    ModelState["ConfirmPassword"].Errors.Clear();
            }
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("FirstName", "اسم العامل مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (City != null && (City.Country.NameEn.ToLower() == "egypt" || City.Country.NameAr.ToLower() == "مصر"))
                    {
                        string EgyptPhoneNumberSyntax = @"^01[0-2|5]{1}[0-9]{8}";
                        bool IsPhoneNumber = Regex.IsMatch(model.PhoneNumber, EgyptPhoneNumberSyntax, RegexOptions.IgnoreCase);
                        if (IsPhoneNumber == true)
                        {
                            if (model.PhoneNumber.Length != 11)
                            {
                                ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                        }
                    }
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                    else
                    {
                        var user = db.Users.Any(d => d.PhoneNumber.ToLower() == model.PhoneNumber.ToLower());
                        if (user == true)
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
                }

                if (User.IsInRole("Admin") == true || User.IsInRole("SubAdmin") == true)
                {
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var user = db.Users.Any(d => d.Email.ToLower() == model.Email.ToLower());
                        if (user == true)
                        {
                            ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");
                    }
                    if (string.IsNullOrEmpty(model.Password))
                        ModelState.AddModelError("Password", "كلمه السر مطلوبة");

                    if (string.IsNullOrEmpty(model.ConfirmPassword))
                        ModelState.AddModelError("ConfirmPassword", "تأكيد كلمه السر مطلوبة");

                    if (model.Password != model.ConfirmPassword)
                        ModelState.AddModelError("Password", "كلمه السر غير متطابقة");
                }
                else
                {
                    if (ModelState.ContainsKey("Email"))
                        ModelState["Email"].Errors.Clear();
                    if (ModelState.ContainsKey("Password"))
                        ModelState["Password"].Errors.Clear();
                    if (ModelState.ContainsKey("ConfirmPassword"))
                        ModelState["ConfirmPassword"].Errors.Clear();
                }
            }

            return ModelState;
        }

        [HttpGet]
        public ActionResult EditWorker(long? WorkerId)
        {
            if (WorkerId.HasValue == true)
            {
                var Worker = db.Workers.Find(WorkerId.Value);
                if (Worker != null)
                {
                    ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
                    return View(EditWorkerVM.ToEditWorkerVM(Worker));
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<ActionResult> OrderCancel(long? OrderId)
        {
            if (await db.JobOrders.AnyAsync(x => x.Id == OrderId))
            {
                var canceldOrder = await db.JobOrders.FirstOrDefaultAsync(x => x.Id == OrderId);
                canceldOrder.Status = JobOrderStatus.CancelledByAdmin;
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Orders));
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChangeWorker(int? OrderId, string ReturnUrl)
        {

            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue)
            {
                ViewBag.Drivers = new SelectList(db.Workers.Include(x => x.User).ToList(), "Id", "User.Name");
                ChangeDriver model = new ChangeDriver()
                {
                    OrderId = Convert.ToInt32(OrderId)
                };
                return View(model);
            }
            return Redirect(ReturnUrl);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ChangeWorker(ChangeDriver model)
        {
            if (model.DriverId != 0)
            {
                var order = await db.JobOrders.FirstOrDefaultAsync(x => x.Id == model.OrderId);
                order.WorkerId = model.DriverId;
                if (await db.SaveChangesAsync() > 0)
                { }
            }
            return RedirectToAction(nameof(Orders));
        }
        [HttpPost]
        public async Task<ActionResult> EditWorker(EditWorkerVM model)
        {
            var Validation = ValidateWorkerEdition(model, ModelState);
            if (Validation.IsValid == false)
            {
                goto Jump;
            }

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            using (var Transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var City = db.Cities.Find(model.CityId);
                    var Worker = db.Workers.Find(model.WorkerId);
                    Worker.DescriptionEn = model.DescriptionEn;
                    Worker.DescriptionAr = model.DescriptionAr;

                    if (model.PersonalPhoto != null)
                    {
                        MediaControl.Delete(FilePath.Users, Worker.User.ImageUrl);
                        Worker.User.ImageUrl = MediaControl.Upload(FilePath.Users, model.PersonalPhoto);
                    }
                    if (model.IdentityPhoto != null)
                    {
                        MediaControl.Delete(FilePath.Users, Worker.IdentityPhoto);
                        Worker.IdentityPhoto = MediaControl.Upload(FilePath.Users, model.IdentityPhoto);
                    }

                    Worker.User.Name = model.Name;
                    Worker.User.Email = model.Email;
                    Worker.User.UserName = model.Email;
                    Worker.User.PhoneNumber = model.PhoneNumber;
                    Worker.User.CityId = model.CityId;
                    Worker.Profit = model.Profit;

                    CRUD<Worker>.Update(Worker);
                    db.SaveChanges();
                    Transaction.Commit();
                    TempData["Success"] = true;
                    return RedirectToAction("Workers");
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    ModelState.AddModelError("", "شئ ما خطأ ، برجاء مراجعة المطور");
                    goto Jump;
                }
            }

            Jump:
            ViewBag.Cities = db.Cities.Where(d => d.IsDeleted == false && d.Country.IsDeleted == false).ToList();
            return View(model);
        }

        private ModelStateDictionary ValidateWorkerEdition(EditWorkerVM model, ModelStateDictionary ModelState)
        {
            if (model.IdentityPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.IdentityPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("IdentityPhoto", "صوره هوية العامل غير صحيحة");
            }

            if (model.PersonalPhoto != null)
            {
                bool IsImageValid = CheckFiles.IsImage(model.PersonalPhoto);
                if (IsImageValid == false)
                    ModelState.AddModelError("PersonalPhoto", "صوره العامل الشخصية غير صحيحة");
            }

            var Worker = db.Workers.Find(model.WorkerId);
            if (Worker == null)
            {
                ModelState.AddModelError("", "العامل المطلوب غير متاح");
            }
            else
            {
                if (string.IsNullOrEmpty(model.Name))
                    ModelState.AddModelError("Name", "اسم العامل مطلوب");

                var City = db.Cities.FirstOrDefault(d => d.Id == model.CityId && d.IsDeleted == false && d.Country.IsDeleted == false);
                if (City == null)
                    ModelState.AddModelError("CityId", "المدينة المطلوبة غير متاحه");

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    if (City != null && (City.Country.NameEn.ToLower() == "egypt" || City.Country.NameAr.ToLower() == "مصر"))
                    {
                        string EgyptPhoneNumberSyntax = @"^01[0-2|5]{1}[0-9]{8}";
                        bool IsPhoneNumber = Regex.IsMatch(model.PhoneNumber, EgyptPhoneNumberSyntax, RegexOptions.IgnoreCase);
                        if (IsPhoneNumber == true)
                        {
                            if (model.PhoneNumber.Length != 11)
                            {
                                ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                        }
                    }
                    if (model.PhoneNumber.StartsWith("+") || !model.PhoneNumber.All(char.IsDigit))
                        ModelState.AddModelError("PhoneNumber", "رقم الهاتف غير صحيح");
                    else
                    {
                        if (UserValidation.IsPhoneExists(model.PhoneNumber, Worker.UserId) == true)
                        {
                            ModelState.AddModelError("PhoneNumber", "رقم الهاتف مُسجل من قبل");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("PhoneNumber", "رقم الهاتف مطلوب");
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    if (UserValidation.IsEmailExists(model.Email, Worker.UserId) == true)
                    {
                        ModelState.AddModelError("Email", "البريد الالكترونى مُسجل من قبل");
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "البريد الالكترونى مطلوب");
                }
            }

            return ModelState;
        }

        public ActionResult Control(long? WorkerId)
        {
            if (WorkerId.HasValue == true)
            {
                var Worker = db.Workers.Find(WorkerId.Value);
                if (Worker != null)
                {
                    return View(Worker);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> ToggleStopWorker(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Worker = db.Workers.Find(Id.Value);
            if (Worker == null || Worker.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Worker.IsBlocked == true)
            {
                Worker.IsBlocked = false;
                CRUD<Worker>.Update(Worker);
                await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
            }
            else
            {
                Worker.IsBlocked = true;
                CRUD<Worker>.Update(Worker);
                await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { WorkerId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> ToggleDeleteWorker(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Worker = db.Workers.Find(Id.Value);
            if (Worker == null || Worker.IsAccepted.HasValue == false)
                return RedirectToAction("Index");

            if (Worker.IsDeleted == true)
            {
                await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "تم رفع ايقافك", "لقد تم رفع ايقافك عن التطبيق", NotificationType.General, IsDriver: true);
                CRUD<Worker>.Restore(Worker);
            }
            else
            {
                await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "تم ايقافك", "لقد تم ايقافك عن العمل فى التطبيق", NotificationType.General, IsDriver: true);
                CRUD<Worker>.Delete(Worker);
            }
            db.SaveChanges();
            return RedirectToAction("Control", new { WorkerId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> AcceptWorker(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Worker = db.Workers.Find(Id.Value);
            if (Worker == null)
                return RedirectToAction("Index");

            Worker.IsAccepted = true;
            CRUD<Worker>.Update(Worker);
            db.SaveChanges();
            await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "تم قبولك", "تهانينا ، لقد تم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "تم قبولك", "تهانينا ، لقد تم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            return RedirectToAction("Control", new { WorkerId = Id });
        }

        [HttpGet]
        public async Task<ActionResult> RejectWorker(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var Worker = db.Workers.Find(Id.Value);
            if (Worker == null)
                return RedirectToAction("Index");

            Worker.IsAccepted = false;
            CRUD<Worker>.Update(Worker);
            db.SaveChanges();
            await Notifications.SendToAllSpecificAndroidUserDevices(Worker.UserId, "لم يتم قبولك", "لم يتم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            await Notifications.SendToAllSpecificIOSUserDevices(Worker.UserId, "لم يتم قبولك", "لم يتم قبولك للعمل فى التطبيق", NotificationType.General, IsDriver: true);
            return RedirectToAction("Control", new { WorkerId = Id });
        }

        public ActionResult WorkerDetails(long? WorkerId)
        {
            if (WorkerId.HasValue == true)
            {
                var Worker = db.Workers.Find(WorkerId.Value);
                if (Worker != null)
                {
                    return View(Worker);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ChangeWorkerPassword(long? WorkerId)
        {
            if (WorkerId.HasValue == true)
            {
                var Worker = db.Workers.Find(WorkerId.Value);
                if (Worker != null)
                {
                    ViewBag.Worker = Worker;
                    return View();
                }
            }
            else
            {
                var Worker = db.Workers.FirstOrDefault(w => w.UserId == CurrentUserId);
                if (Worker != null)
                {
                    ViewBag.Worker = Worker;
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangeWorkerPassword(ChangePasswordVM model)
        {
            if (User.IsInRole("Worker") == false)
            {
                if (ModelState.ContainsKey("CurrentPassword"))
                {
                    ModelState["CurrentPassword"].Errors.Clear();
                }
            }
            if (ModelState.IsValid == true)
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var Worker = db.Workers.Find(model.DriverId);
                if (Worker != null)
                {
                    Worker.User.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    db.SaveChanges();
                    TempData["Success"] = true;
                    if (User.IsInRole("Worker"))
                    {
                        return null;
                        //return RedirectToAction("Profile");
                    }
                    return RedirectToAction("Workers");
                }
            }
            ViewBag.Worker = db.Workers.Find(model.DriverId);
            return View(model);
        }

        [HttpGet]
        public ActionResult WorkerJobs(long? WorkerId)
        {
            if (WorkerId.HasValue == true)
            {
                var Worker = db.Workers.Find(WorkerId.Value);
                if (Worker != null)
                {
                    ViewBag.Jobs = db.Jobs.Where(s => s.IsDeleted == false).ToList();
                    return View(Worker);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ToggleDeleteWorkerJob(long? Id)
        {
            if (Id.HasValue == false)
                return RedirectToAction("Index");

            var WorkerJob = db.JobWorkers.Find(Id.Value);
            if (WorkerJob == null)
                return RedirectToAction("Index");

            if (WorkerJob.IsDeleted == true)
            {
                CRUD<JobWorker>.Restore(WorkerJob);
            }
            else
            {
                CRUD<JobWorker>.Delete(WorkerJob);
            }
            db.SaveChanges();
            return RedirectToAction("WorkerJobs", new { WorkerId = WorkerJob.WorkerId });
        }

        [HttpPost]
        public ActionResult AddWorkerJob(long Id, long[] JobIds)
        {
            var Worker = db.Workers.Find(Id);
            if (Worker != null && JobIds != null)
            {
                foreach (var job in JobIds)
                {
                    var Job = db.Jobs.FirstOrDefault(w => w.IsDeleted == false && w.Id == job);
                    if (Job != null)
                    {
                        var WorkerJob = db.JobWorkers.FirstOrDefault(w => w.JobId == job && w.WorkerId == Id);
                        if (WorkerJob == null)
                        {
                            db.JobWorkers.Add(new JobWorker()
                            {
                                JobId = job,
                                WorkerId = Id
                            });
                        }
                        else
                        {
                            if (WorkerJob.IsDeleted == true)
                            {
                                CRUD<JobWorker>.Restore(WorkerJob);
                            }
                        }
                    }
                }
                db.SaveChanges();
                TempData["Success"] = true;
                return RedirectToAction("WorkerJobs", new { WorkerId = Worker.Id });
            }
            return RedirectToAction("Workers");
        }

        [HttpGet]
        public ActionResult Orders()
        {
            var Orders = db.JobOrders.Where(s => s.IsDeleted == false).OrderByDescending(s => s.CreatedOn).ToList();
            return View(Orders);
        }

        public ActionResult Chat(long? OrderId, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (OrderId.HasValue == true)
            {
                var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return Redirect(ReturnUrl);
        }

        [HttpGet]
        public async Task<ActionResult> RejectOrder(long? OrderId)
        {
            if (OrderId.HasValue == true)
            {
                var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId && (w.Status == JobOrderStatus.AcceptedByWorker || w.Status == JobOrderStatus.Placed));
                if (Order != null)
                {
                    CRUD<JobOrder>.Update(Order);
                    Order.Status = JobOrderStatus.CancelledByAdmin;
                    db.SaveChanges();
                    await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, $"تم الغاء طلبك رقم {Order.Code}", "لقد تم الغاء الطلب الخاص بكم من قبل الاداره", NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, $"تم الغاء طلبك رقم {Order.Code}", "لقد تم الغاء الطلب الخاص بكم من قبل الاداره", NotificationType.ApplicationJobOrderDetails, Order.Id, false, true);
                    TempData["Success"] = true;
                }
            }
            return RedirectToAction("Orders");
        }

        [HttpGet]
        public ActionResult OrderDetails(long? OrderId)
        {
            if (OrderId.HasValue == true)
            {
                var Order = db.JobOrders.FirstOrDefault(w => w.IsDeleted == false && w.Id == OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return RedirectToAction("Orders");
        }
    }
}
