using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class JobWorkerDTO
    {
        public string JobName { get; set; }
        public int NumberOfWorkers { get; set; }
        public List<WorkerDTO> Workers { get; set; } = new List<WorkerDTO>();
    }
}