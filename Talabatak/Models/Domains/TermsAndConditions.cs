using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Talabatak.Models.Domains
{
    public class TermsAndConditions: BaseModel
    {
        [AllowHtml]
        public string Terms_Conditions { get; set; }

    }
}