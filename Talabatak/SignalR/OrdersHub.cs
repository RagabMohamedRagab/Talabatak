using Talabatak.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.SignalR
{
    [Authorize]
    public class OrdersHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void DriversJoin()
        {
            string CurrentUserId = Context.User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Driver != null)
            {
                Groups.Add(Context.ConnectionId, Driver.Id.ToString());
                Groups.Add(Context.ConnectionId, "Drivers");
            }
        }

        public void WorkersJoin()
        {
            string CurrentUserId = Context.User.Identity.GetUserId();
            var Worker = db.Workers.FirstOrDefault(s => s.UserId == CurrentUserId);
            if (Worker != null)
            {
                Groups.Add(Context.ConnectionId, Worker.Id.ToString());
                Groups.Add(Context.ConnectionId, "Workers");
            }
        }

        public void JoinDashboardPage()
        {
            Groups.Add(Context.ConnectionId, "StoreOrdersDashboard");
            Groups.Add(Context.ConnectionId, "OtlobAy7agaOrdersDashboard");
        }
    }
}