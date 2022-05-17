using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class WorkerProfileDTO
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Rate { get; set; }
        public List<GetWorkerReviewDTO> Reviews { get; set; } = new List<GetWorkerReviewDTO>();
    }
}