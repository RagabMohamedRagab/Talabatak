using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.SignalR;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Talabatak.Helpers
{
    public class OtlobAy7agaOrderActions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<Driver> GetAvailableDrivers(long CityId, double Latitude, double Longitude, long? OrderId = 0)
        {
            var City = db.Cities.Find(CityId);
            if (City != null)
            {
                var Drivers = db.Drivers.Where(d => d.User.CityId == CityId && d.IsAccepted == true && d.IsAvailable == true && d.IsDeleted == false && d.IsOnline == true && d.IsBlocked == false && d.User.Latitude.HasValue && d.User.Longitude.HasValue).Take(15).ToList();
                if (OrderId.HasValue == true && OrderId.Value != 0)
                {
                    var UnwantedDrivers = db.OtlobAy7agaOrderDrivers.Where(s => s.OrderId == OrderId.Value && s.IsDeleted == false && s.IsAccepted == false).Select(w => w.DriverId).ToList();
                    Drivers = Drivers.Where(s => UnwantedDrivers.Contains(s.Id) == false).ToList();
                }
                return Drivers.OrderBy(d => Distance.GetDistanceBetweenTwoLocations(Latitude, Longitude, d.User.Latitude.Value, d.User.Longitude.Value)).Take(15).ToList();
            }
            return new List<Driver>();
        }

        public static async Task SendOrderToDriver(long OrderId, Driver Driver)
        {
            try
            {
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                var OrderDriver = db.OtlobAy7agaOrderDrivers.FirstOrDefault(w => w.IsDeleted == false && w.OrderId == OrderId && w.DriverId == Driver.Id);
                if (OrderDriver == null)
                {
                    OtlobAy7agaOrderDriver driver = new OtlobAy7agaOrderDriver()
                    {
                        DriverId = Driver.Id,
                        OrderId = OrderId,
                    };
                    db.OtlobAy7agaOrderDrivers.Add(driver);
                    db.SaveChanges();
                    string Title = $"طلب توصيل جديد";
                    string Body = "يوجد طلب جديد من قسم [اطلب اى حاجه] يرجى المراجعه";
                    await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationPendingOtlobAy7agaOrdersPage, OrderId, true, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationPendingOtlobAy7agaOrdersPage, OrderId, true, true);
                    OrdersHub.Clients.Group(driver.Id.ToString()).newOrder();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static decimal CalculateDeliveryFees(int Kilometers)
        {
            decimal DeliveryFees = 0;
            var MetaData = db.MetaDatas.FirstOrDefault(s => s.IsDeleted == false);
            if (MetaData == null)
            {
                DeliveryFees = 0;
            }
            else
            {
                decimal DeliveryOpenFarePrice = MetaData.OtlobAy7agaDeliveryOpenFarePrice;
                double DeliveryOpenFareKilometers = MetaData.OtlobAy7agaDeliveryOpenFareKilometers;
                decimal DeliveryEveryKilometerPrice = MetaData.OtlobAy7agaDeliveryEveryKilometerPrice;
                decimal DeliveryStaticExtraFees = MetaData.OtlobAy7agaDeliveryStaticExtraFees;

                var ExactDistanceAfterOpenKilometers = (decimal)(Kilometers / 1000.00) - (decimal)DeliveryOpenFareKilometers;
                decimal KilometersCost = ExactDistanceAfterOpenKilometers * DeliveryEveryKilometerPrice;
                if ((Kilometers / 1000) <= DeliveryOpenFareKilometers)
                {
                    DeliveryFees = DeliveryOpenFarePrice;
                }
                else
                {
                    DeliveryFees = DeliveryOpenFarePrice + KilometersCost;
                }
                DeliveryFees += DeliveryStaticExtraFees;
            }
            return DeliveryFees;
        }
    }
}