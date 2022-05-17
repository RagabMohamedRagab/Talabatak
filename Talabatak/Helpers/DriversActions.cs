using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Helpers
{
    public static class DriversActions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static int GetDriverOrdersCount(Driver driver)
        {
            if (driver != null)
            {
                var OtlobAy7agaOrdersCount = db.OtlobAy7agaOrders.Count(s => s.IsDeleted == false && (s.OrderStatus == OtlobAy7agaOrderStatus.Placed || s.OrderStatus == OtlobAy7agaOrderStatus.Started) && s.DriverId == driver.Id);
                var StoreOrdersCount = db.StoreOrders.Count(s => s.IsDeleted == false && s.DriverId == driver.Id && s.Status == StoreOrderStatus.Delivering);
                return OtlobAy7agaOrdersCount + StoreOrdersCount;
            }
            return 0;
        }

        public static void SetDriverAvailability(Driver driver)
        {
            var MetaData = db.MetaDatas.FirstOrDefault(w => w.IsDeleted == false);
            if (driver != null && MetaData != null)
            {
                int OrdersCount = GetDriverOrdersCount(driver);
                if (OrdersCount >= MetaData.NumberOfAvailableOrdersPerDriver)
                {
                    driver.IsAvailable = false;
                }
                else
                {
                    driver.IsAvailable = true;
                }
                db.SaveChanges();
            }
        }
    }
}