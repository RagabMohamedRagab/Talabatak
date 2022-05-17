using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace Talabatak.SignalR
{
    [Authorize]
    public class TrackingHub : Hub
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void DriverJoin()
        {
            string CurrentUserId = Context.User.Identity.GetUserId();
            Groups.Add(Context.ConnectionId, CurrentUserId);
        }

        public void LogDriverLocation(double Latitude, double Longitude)
        {
            string CurrentUserId = Context.User.Identity.GetUserId();
            var Driver = db.Drivers.FirstOrDefault(x => x.UserId == CurrentUserId);
            try
            {
                Driver.User.Latitude = Latitude;
                Driver.User.Longitude = Longitude;
                db.SaveChanges();
            }
            catch (Exception)
            {
            }
            Clients.Group(Driver.Id.ToString()).broadCastLocation(Latitude, Longitude);
        }

        public void UserJoin(long DriverId)
        {
            Groups.Add(Context.ConnectionId, DriverId.ToString());
        }
    }
}