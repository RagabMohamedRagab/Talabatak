using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class DriverReportDto
    {
        public int MyProperty { get; set; }
        public double Wallet { get; set; }
        public int OrdersCount { get; set; }
        public int Rate { get; set; }
        public double PaiedForSys { get; set; }
        public int ReviewCounter { get; set; }
    }
}