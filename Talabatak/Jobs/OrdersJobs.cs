using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Microsoft.AspNet.SignalR;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace Talabatak.Jobs
{
    public class OrdersJobs : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var PendingStoreOrders = db.StoreOrders.Where(d => d.IsDeleted == false && d.Status == StoreOrderStatus.Preparing && d.DriverId.HasValue == false).ToList();
                var PendingOtlobOrders = db.OtlobAy7agaOrders.Where(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Placed && d.DriverId.HasValue == false).ToList();
                var MetaData = db.MetaDatas.FirstOrDefault(d => d.IsDeleted == false);
                var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                int NumberOfMinutesBeforeCancellingOrderWithoutDriver = 10;
                if (MetaData != null)
                {
                    NumberOfMinutesBeforeCancellingOrderWithoutDriver = MetaData.NumberOfMinutesBeforeCancellingOrderWithoutDriver;
                }

                foreach (var Order in PendingStoreOrders)
                {
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    if (Order.ModifiedOn.HasValue == true)
                    {
                        CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.ModifiedOn.Value, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    }
                    var DateDifference = DateTimeNow - CreatedOn;
                    var Timer = TimeSpan.FromMinutes(NumberOfMinutesBeforeCancellingOrderWithoutDriver);
                    var TimeLeftInSeconds = (int)(Timer - DateDifference).TotalSeconds;

                    if (TimeLeftInSeconds <= 0)
                    {
                        if (Order.IsPaid == true)
                        {
                            if (Order.PaymentMethod == PaymentMethod.Online)
                            {
                                Order.IsRefundRequired = true;
                            }
                            if (Order.PaymentMethod == PaymentMethod.Wallet)
                            {
                                Order.User.Wallet += Order.Total;
                                Order.IsPaid = false;
                                db.UserWallets.Add(new UserWallet()
                                {
                                    StoreOrderId = Order.Id,
                                    TransactionAmount = Order.Total,
                                    TransactionType = TransactionType.RefundTheUser,
                                    UserId = Order.UserId
                                });
                            }
                        }
                        Order.Status = StoreOrderStatus.Cancelled;
                        var AnotherDrivers = db.StoreOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == Order.Id).ToList();
                        var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<SignalR.OrdersHub>();
                        OrdersHub.Clients.Group("StoreOrdersDashboard").update(Order.Id);
                        foreach (var driver in AnotherDrivers)
                        {
                            OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // to remove the order from his page
                        }
                        await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, $"تم الغاء الطلب رقم {Order.Code}", "نعتذر لكم عن الغاء طلبكم وذلك لعدم توافر سائقين فى الوقت الحالى", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                        await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, $"تم الغاء الطلب رقم {Order.Code}", "نعتذر لكم عن الغاء طلبكم وذلك لعدم توافر سائقين فى الوقت الحالى", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                    }
                    else
                    {
                        var Item = Order.Items.FirstOrDefault(s => s.IsDeleted == false);
                        Store store = null;
                        if (Item != null)
                        {
                            store = Item.Product.Category.Store;
                        }
                        var Drivers = StoreOrderActions.GetAvailableDrivers(store, Order.Id);
                        if (Drivers != null)
                        {
                            foreach (var driver in Drivers)
                            {
                                await StoreOrderActions.AssignOrderToDriverAsync(Order, driver);
                            }
                        }
                    }
                    db.SaveChanges();
                }

                foreach (var Order in PendingOtlobOrders)
                {
                    DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    var DateDifference = DateTimeNow - CreatedOn;
                    var Timer = TimeSpan.FromMinutes(NumberOfMinutesBeforeCancellingOrderWithoutDriver);
                    var TimeLeftInSeconds = (int)(Timer - DateDifference).TotalSeconds;

                    if (TimeLeftInSeconds <= 0)
                    {
                        if (Order.IsPaid == true)
                        {
                            Order.IsRefundRequired = true;
                        }
                        Order.OrderStatus = OtlobAy7agaOrderStatus.Cancelled;
                        var AnotherDrivers = db.OtlobAy7agaOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == Order.Id).ToList();
                        var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<SignalR.OrdersHub>();
                        OrdersHub.Clients.Group("OtlobAy7agaOrdersDashboard").update(Order.Id);
                        foreach (var driver in AnotherDrivers)
                        {
                            OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // to remove the order from his page
                        }
                        await Notifications.SendToAllSpecificAndroidUserDevices(Order.UserId, $"تم الغاء الطلب رقم {Order.Code}", "نعتذر لكم عن الغاء طلبكم وذلك لعدم توافر سائقين فى الوقت الحالى", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                        await Notifications.SendToAllSpecificIOSUserDevices(Order.UserId, $"تم الغاء الطلب رقم {Order.Code}", "نعتذر لكم عن الغاء طلبكم وذلك لعدم توافر سائقين فى الوقت الحالى", NotificationType.ApplicationStoreOrderDetails, Order.Id, false, true);
                    }
                    else
                    {
                        var Drivers = OtlobAy7agaOrderActions.GetAvailableDrivers(Order.CityId, Order.UserAddress.Latitude.Value, Order.UserAddress.Longitude.Value, Order.Id);
                        if (Drivers != null && Drivers.Count() > 0)
                        {
                            foreach (var driver in Drivers)
                            {
                                await OtlobAy7agaOrderActions.SendOrderToDriver(Order.Id, driver);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                db.SaveChanges();
            }
        }

        public static async Task RunProgram()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<OrdersJobs>()
                    .WithIdentity("job2", "group2")
                    .Build();

                // Trigger the job to run now, and then repeat every n seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group2")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(5)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);

                // some sleep to show what's happening

                // and last shut down the scheduler when you are ready to close your program
            }
            catch (SchedulerException se)
            {
            }
        }
    }
}