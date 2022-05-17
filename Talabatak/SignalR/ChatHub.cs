using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Talabatak.SignalR
{
    [Authorize]
    [HubName("ChatHub")]
    public class ChatHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task Join(long OrderId)
        {
            await Groups.Add(Context.ConnectionId, OrderId.ToString());
        }

        public async Task SendMessage(bool IsDriver, long ChatId, string Message, OrderType OrderType)
        {
            // ChatId = OrderId
            string CurrentUserId = Context.User.Identity.GetUserId();
            if (OrderType == OrderType.OtlobAy7aga)
            {
                var Order = db.OtlobAy7agaOrders.Find(ChatId);
                if (Order != null && Order.DriverId.HasValue == true)
                {
                    string ToUserId = null;
                    OtlobAy7agaOrderChat msg = new OtlobAy7agaOrderChat()
                    {
                        FromUserId = CurrentUserId,
                        Message = Message,
                        OrderId = ChatId,
                    };
                    if (IsDriver == false)
                    {
                        msg.ToUserId = Order.Driver.UserId;
                        ToUserId = Order.Driver.UserId;
                    }
                    else
                    {
                        msg.ToUserId = Order.UserId;
                        ToUserId = Order.UserId;
                    }
                    db.OtlobAy7agaOrderChats.Add(msg);
                    db.SaveChanges();
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    Clients.Group(ChatId.ToString()).broadCastMessage(Message, IsDriver, "", CreatedOn.ToString("MMM dd, hh:mm tt"));

                    Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, Message, "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !IsDriver, false);
                    Notifications.SendToAllSpecificIOSUserDevices(ToUserId, Message, "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !IsDriver, false);
                }
            }
            if(OrderType == OrderType.StoreOrder)
            {
                var Order = db.StoreOrders.Find(ChatId);
                if (Order != null && Order.DriverId.HasValue == true)
                {
                    string ToUserId = null;
                    StoreOrderChat msg = new StoreOrderChat()
                    {
                        FromUserId = CurrentUserId,
                        Message = Message,
                        OrderId = ChatId,
                    };
                    if (IsDriver == false)
                    {
                        msg.ToUserId = Order.Driver.UserId;
                        ToUserId = Order.Driver.UserId;
                    }
                    else
                    {
                        msg.ToUserId = Order.UserId;
                        ToUserId = Order.UserId;
                    }
                    db.StoreOrderChats.Add(msg);
                    db.SaveChanges();
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    Clients.Group(ChatId.ToString()).broadCastMessage(Message, IsDriver, "", CreatedOn.ToString("MMM dd, hh:mm tt"));

                    await Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, Message, "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !IsDriver, false);
                    await Notifications.SendToAllSpecificIOSUserDevices(ToUserId, Message, "", NotificationType.ApplicationStoreOrderChatPage, Order.Id, !IsDriver, false);
                }
            }
            if (OrderType == OrderType.JobOrder)
            {
                var Order = db.JobOrders.Find(ChatId);
                if (Order != null)
                {
                    string ToUserId = null;
                    JobOrderChat msg = new JobOrderChat()
                    {
                        FromUserId = CurrentUserId,
                        Message = Message,
                        OrderId = ChatId,
                    };
                    if (IsDriver == false)
                    {
                        msg.ToUserId = Order.Worker.UserId;
                        ToUserId = Order.Worker.UserId;
                    }
                    else
                    {
                        msg.ToUserId = Order.UserId;
                        ToUserId = Order.UserId;
                    }
                    db.JobOrderChats.Add(msg);
                    db.SaveChanges();
                    var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(msg.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                    Clients.Group(ChatId.ToString()).broadCastMessage(Message, IsDriver, "", CreatedOn.ToString("MMM dd, hh:mm tt"));

                    await Notifications.SendToAllSpecificAndroidUserDevices(ToUserId, Message, "", NotificationType.ApplicationJobOrderChatPage, Order.Id, !IsDriver, false);
                    await Notifications.SendToAllSpecificIOSUserDevices(ToUserId, Message, "", NotificationType.ApplicationJobOrderChatPage, Order.Id, !IsDriver, false);
                }
            }
        }
    }
}