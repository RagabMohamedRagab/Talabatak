using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using Talabatak.Models.ViewModels;

namespace Talabatak.Controllers.MVC
{
    public class TermsAndConditionsController : BaseController
    {
        [Authorize(Roles = "Admin, SubAdmin")]
        // GET: TermsAndConditions
        public ActionResult Index()
        {
            TermsAndConditions termsAndConditions = new TermsAndConditions(); 
            if(db.TermsAndConditions.Any() == true);
            {
                termsAndConditions = db.TermsAndConditions.FirstOrDefault();
            }
            return View(termsAndConditions);
        }
        [HttpPost]
        public ActionResult Save(TermsAndConditions model)
        {
            if (db.TermsAndConditions.Any() == true)
            {
                TermsAndConditions termsAndConditions = db.TermsAndConditions.FirstOrDefault();
                if (ModelState.IsValid)
                {
                    termsAndConditions.Id = model.Id;
                    termsAndConditions.Terms_Conditions = model.Terms_Conditions;

                    db.Entry(termsAndConditions).State = EntityState.Modified;
                    CRUD<TermsAndConditions>.Update(termsAndConditions);
                }
            }
            else
            {
                TermsAndConditions termsAndConditions = new TermsAndConditions();
                termsAndConditions.Terms_Conditions = model.Terms_Conditions;
                db.TermsAndConditions.Add(termsAndConditions);
            }

            db.SaveChanges();
            return View("Index");
        }
        [AllowAnonymous]
        public ActionResult User()
        {
            TermsAndConditions termsAndConditions = new TermsAndConditions();
            if (db.TermsAndConditions.Any() == true) ;
            {
                termsAndConditions = db.TermsAndConditions.FirstOrDefault();
            }
            return View(termsAndConditions);
        }
    }
}

