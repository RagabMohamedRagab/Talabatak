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
    public class StoreOrderActions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<Driver> GetAvailableDrivers(Store Store, long? OrderId = 0)
        {
            if (Store != null && Store.Latitude.HasValue == true && Store.Longitude.HasValue == true)
            {
                var Drivers = db.Drivers.Where(d => d.IsAccepted == true && d.IsAvailable == true && d.IsDeleted == false && d.IsOnline == true && d.IsBlocked == false && d.User.Latitude.HasValue && d.User.Longitude.HasValue && d.User.CityId == Store.CityId).ToList();
                if (OrderId.HasValue == true && OrderId.Value != 0)
                {
                    var UnwantedDrivers = db.StoreOrderDrivers.Where(s => s.OrderId == OrderId.Value && s.IsDeleted == false && s.IsAccepted == false).Select(w => w.DriverId).ToList();
                    Drivers = Drivers.Where(s => UnwantedDrivers.Contains(s.Id) == false).ToList();
                }
                return Drivers.OrderBy(d => Distance.GetDistanceBetweenTwoLocations(Store.Latitude.Value, Store.Longitude.Value, d.User.Latitude.Value, d.User.Longitude.Value)).Take(15).ToList();
            }
            return new List<Driver>();
        }
        public static async Task CalculateDeliveryFeesForStore(StoreOrder order, Store store)
        {
            decimal DeliveryFees = 0;
            var RandomOrderItem = order.Items.FirstOrDefault(s => s.IsDeleted == false);
            if (RandomOrderItem != null && order.UserAddress != null)
            {
                var Store = RandomOrderItem.Product.Category.Store;
                var ExactDistanceBetweenStoreAndUser = await Distance.GetEstimatedDataBetweenTwoLocations(Store.Latitude.Value, Store.Longitude.Value, order.UserAddress.Latitude.Value, order.UserAddress.Longitude.Value);
                decimal DeliveryOpenFarePrice = store.StoreOrdersDeliveryOpenFarePrice;
                double DeliveryOpenFareKilometers = store.StoreOrdersDeliveryOpenFareKilometers;
                decimal DeliveryEveryKilometerPrice = store.StoreOrdersDeliveryEveryKilometerPrice;

                var ExactDistanceAfterOpenKilometers = (decimal)(ExactDistanceBetweenStoreAndUser.TotalEstimatedDistance / 1000.00) - (decimal)DeliveryOpenFareKilometers;
                decimal KilometersCost = ExactDistanceAfterOpenKilometers * DeliveryEveryKilometerPrice;
                if ((ExactDistanceBetweenStoreAndUser.TotalEstimatedDistance / 1000.00) <= DeliveryOpenFareKilometers)
                {
                    DeliveryFees = DeliveryOpenFarePrice;
                }
                else
                {
                    DeliveryFees = DeliveryOpenFarePrice + KilometersCost;
                }
                order.DeliveryFees = DeliveryFees;
                order.IsDeliveryFeesUpdated = true;
            }
            else
            {
                order.DeliveryFees = 0;
                order.IsDeliveryFeesUpdated = false;
            }
        }
        public static async Task AssignOrderToDriverAsync(StoreOrder Order, Driver driver)
        {
            if (Order != null && driver != null)
            {
                try
                {
                    var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                    var OrderDriver = db.StoreOrderDrivers.FirstOrDefault(s => s.IsDeleted == false && s.OrderId == Order.Id && s.DriverId == driver.Id);
                    if (!(db.StoreOrderDrivers.Any(s => s.IsDeleted == false && s.OrderId == Order.Id && s.DriverId == driver.Id)))
                    {
                        db.StoreOrderDrivers.Add(new StoreOrderDriver()
                        {
                            DriverId = driver.Id,
                            OrderId = Order.Id,
                        });
                        db.SaveChanges();
                        var Store = Order.Items.FirstOrDefault(w => w.IsDeleted == false).Product.Category.Store;
                        string Title = $"مقترح طلب توصيل جديد";
                        string Body = $"توصيل طلب من {Store.NameAr} ، راجع التطبيق";
                        await Notifications.SendToAllSpecificAndroidUserDevices(driver.UserId, Title, Body, NotificationType.ApplicationPendingStoreOrdersPage, Order.Id, true, true);
                        await Notifications.SendToAllSpecificIOSUserDevices(driver.UserId, Title, Body, NotificationType.ApplicationPendingStoreOrdersPage, Order.Id, true, true);
                        OrdersHub.Clients.Group(driver.Id.ToString()).newOrder();
                    }
                }
            catch (Exception)
            {
            }
            }
        }

        public static void CalculateOrderPrice(StoreOrder order)
        {
            order.SubTotal = order.Items.Where(x => x.IsDeleted == false).Sum(d => d.SubTotal);
            order.Total = order.SubTotal + order.DeliveryFees;
        }

        public static async Task CalculateDeliveryFees(StoreOrder order)
        {
            if (order.UserAddressId.HasValue == false)
            {
                order.DeliveryFees = 0;
                order.IsDeliveryFeesUpdated = false;
            }
            else
            {
                if (order.IsDeliveryFeesUpdated == false)
                {
                    var MetaData = db.MetaDatas.FirstOrDefault(s => s.IsDeleted == false);
                    if (MetaData == null)
                    {
                        order.DeliveryFees = 0;
                        order.IsDeliveryFeesUpdated = false;
                    }
                    else
                    {
                        decimal DeliveryFees = 0;
                        var RandomOrderItem = order.Items.FirstOrDefault(s => s.IsDeleted == false);
                        if (RandomOrderItem != null)
                        {
                            var Store = RandomOrderItem.Product.Category.Store;
                            var ExactDistanceBetweenStoreAndUser = await Distance.GetEstimatedDataBetweenTwoLocations(Store.Latitude.Value, Store.Longitude.Value, order.UserAddress.Latitude.Value, order.UserAddress.Longitude.Value);

                            decimal DeliveryOpenFarePrice = MetaData.StoreOrdersDeliveryOpenFarePrice;
                            double DeliveryOpenFareKilometers = MetaData.StoreOrdersDeliveryOpenFareKilometers;
                            decimal DeliveryEveryKilometerPrice = MetaData.StoreOrdersDeliveryEveryKilometerPrice;

                            var ExactDistanceAfterOpenKilometers = (decimal)(ExactDistanceBetweenStoreAndUser.TotalEstimatedDistance / 1000.00) - (decimal)DeliveryOpenFareKilometers;
                            decimal KilometersCost = ExactDistanceAfterOpenKilometers * DeliveryEveryKilometerPrice;
                            if ((ExactDistanceBetweenStoreAndUser.TotalEstimatedDistance / 1000.00) <= DeliveryOpenFareKilometers)
                            {
                                DeliveryFees = DeliveryOpenFarePrice;
                            }
                            else
                            {
                                DeliveryFees = DeliveryOpenFarePrice + KilometersCost;
                            }
                            order.DeliveryFees = DeliveryFees;
                            order.IsDeliveryFeesUpdated = true;
                        }
                        else
                        {
                            order.DeliveryFees = 0;
                            order.IsDeliveryFeesUpdated = false;
                        }
                    }
                }
            }
        }
    }
}