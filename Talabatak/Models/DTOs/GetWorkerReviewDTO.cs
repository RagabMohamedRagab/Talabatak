using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class GetWorkerReviewDTO
    {
        public string ReviewerName { get; set; }
        public string ReviewerImageUrl { get; set; }
        public string ReviewDate { get; set; }
        public int ReviewStars { get; set; }
        public string ReviewDescription { get; set; }
    }
}