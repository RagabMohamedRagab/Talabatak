using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.ViewModels
{
    public class HomeReportVM
    {
        public int DriverAvailable { get; set; }
        public int DriverDeleted { get; set; }
        public int DriverBlocked { get; set; }
        public int WorkerAvailable { get; set; }
        public int WorkerDeleted { get; set; }
        public int WorkerBlocked { get; set; }
        public decimal AmdinWallet { get; set; }
        public decimal WorkerIncoming { get; set; }
        public decimal DriverStoreIncoming { get; set; }
        public decimal DriverOtlobIncoming { get; set; }
        public decimal StoreIncoming { get; set; }
        public int StoreAvilable { get; set; }
        public int StoreDeleted { get; set; }
        public int StoreBlocked { get; set; }
        public int DoneDriverOrder { get; set; }
        public int CancelDriverOrder { get; set; }
        public int RejectedDriverOrder { get; set; }
        public int DoneWorkerOrder { get; set; }
        public int CancelWorkerOrder { get; set; }
        public int RejectedWorkerOrder { get; set; }
    }
}