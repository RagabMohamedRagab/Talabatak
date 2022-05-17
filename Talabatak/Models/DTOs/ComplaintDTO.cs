using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class ComplaintDTO
    {
        public string ComplaintReport { get; set; }
        public string Message { get; set; }
    }
}