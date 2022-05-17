using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;
using Talabatak.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Text;
using System.Net;
using System.Configuration;
using System.Web.Hosting;
using System.IO;
using PushSharp.Apple;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Talabatak.Helpers
{
    public static class Notifications
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        //Android
        private static string UserServerKey = ConfigurationManager.AppSettings["USER_SERVER_API_KEY"];
        private static string UserSenderId = ConfigurationManager.AppSettings["USER_SERVER_SENDER_ID"];

        private static string DriverServerKey = ConfigurationManager.AppSettings["DRIVER_SERVER_API_KEY"];
        private static string DriverSenderId = ConfigurationManager.AppSettings["DRIVER_SERVER_SENDER_ID"];

        //IOS
        private static string UserFullPath = HostingEnvironment.MapPath("~") + "/Files/UserCertificate.p12";
        private static string DirverFullPath = HostingEnvironment.MapPath("~") + "/Files/DriverCertificate.p12";
        private static string certPassword = "Project1234";

        public static void SendWebNotification(string Title, string Message, string UserId = null, long StoreId = -1, string Role = null, long Id = -1, NotificationType notificationLinkType = NotificationType.General, bool IsSaveInDatabase = false)
        {
            var Hub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            if (!string.IsNullOrEmpty(UserId))
            {
                Hub.Clients.Group(UserId).notify(Message);
            }
            if (!string.IsNullOrEmpty(Role))
            {
                Hub.Clients.Group(Role).notify(Message);
            }
            if (StoreId > 0)
            {
                Hub.Clients.Group(StoreId.ToString()).notify(Message);
            }
            string Link = null;
            switch (notificationLinkType)
            {
                case NotificationType.AdminStoresPage:
                    Link = "/Stores/Index";
                    break;
                case NotificationType.AdminWorkersPage:
                    Link = "/AppJobs/Workers";
                    break;
                case NotificationType.WebStoreOrderDetailsPage:
                    Link = "/StoreOrders/Details?OrderId=" + Id + "&ReturnUrl=" + HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/StoreOrders";
                    break;
                case NotificationType.WebOtlobAy7agaOrderDetailsPage:
                    Link = "/OtlobAy7aga/Details?OrderId=" + Id + "&ReturnUrl=" + HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/StoreOrders";
                    break;
                case NotificationType.WebWorkerDetailsPage:
                    Link = "/AppJobs/OrderDetails?OrderId=" + Id + "&ReturnUrl=" + HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/StoreOrders";
                    break;

                default:
                    break;
            }
            if (IsSaveInDatabase == true)
            {
                if (!string.IsNullOrEmpty(UserId))
                {
                    SaveInDatabase(UserId, Title, Message, notificationLinkType, Id, Link);
                }
                else if (StoreId > 0)
                {
                    SaveInDatabase(null, Title, Message, notificationLinkType, Id, Link, StoreId);
                }
                else if (!string.IsNullOrEmpty(Role))
                {
                    var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                    var RoleObject = RoleManager.FindByName(Role);
                    List<ApplicationUser> RoleUsers = new List<ApplicationUser>();
                    if (RoleObject != null)
                    {
                        RoleUsers = db.Users.Where(d => d.Roles.Any(x => x.RoleId == RoleObject.Id)).ToList();
                        if (RoleUsers != null && RoleUsers.Count() > 0)
                        {
                            foreach (var user in RoleUsers)
                            {
                                SaveInDatabase(user.Id, Title, Message, notificationLinkType, Id, Link, IsWorker: Role.ToLower() == "driver" ? true : false);
                            }
                        }
                    }
                }
            }
        }
        public async static Task SendToAllSpecificAndroidUserDevices(string UserId, string Title, string Message, NotificationType NotificationType, long Id = -1, bool IsDriver = false, bool IsSaveInDatabase = true)
        {
            var users = db.PushTokens.Where(d => d.OS == OS.Android && d.UserId == UserId && d.IsDeleted == false).ToList();
            foreach (var user in users)
            {
                var message = new
                {
                    to = user.Token,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = Message,
                        title = Title,
                        sound = "default"
                    },
                    data = new
                    {
                        id = Id.ToString(),
                        notificationtype = ((int)NotificationType).ToString(),
                    }
                };

                var Serializer = new JavaScriptSerializer();
                var Json = Serializer.Serialize(message);
                var byteArray = Encoding.UTF8.GetBytes(Json);

                try
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", IsDriver == true ? DriverServerKey : UserServerKey));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", IsDriver == true ? DriverSenderId : UserSenderId));
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    string sResponseFromServer = tReader.ReadToEnd();
                                    string str = sResponseFromServer;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                
            }
            if (IsSaveInDatabase == true)
            {
                SaveInDatabase(UserId, Title, Message, NotificationType, Id, IsWorker: IsDriver);
            }
        }

        public async static Task SendToAllAndroidDevices(string Title, string Message, bool IsDriver = false, bool IsWorker = false, bool IsSaveInDatabase = true)
        {
            List<PushToken> users = new List<PushToken>();
            if (IsDriver == false && IsWorker == false)
            {
                users = db.PushTokens.Where(d => d.OS == OS.Android && d.IsDeleted == false && d.IsWorker == false).ToList();
            }
            if (IsDriver == true)
            {
                users = db.PushTokens.Where(d => d.OS == OS.Android && d.IsDeleted == false && d.IsWorker == true && d.User.Drivers.Any(w => w.IsDeleted == false) == true).ToList();
            }
            if (IsWorker == true)
            {
                users = db.PushTokens.Where(d => d.OS == OS.Android && d.IsDeleted == false && d.IsWorker == true && d.User.Workers.Any(w => w.IsDeleted == false) == true).ToList();
            }
            //var users = db.PushTokens.Where(d => d.OS == OS.Android && d.IsDeleted == false && d.IsWorker == IsDriver).Select(d => new { d.Token, d.UserId }).Distinct().ToList();

            foreach (var user in users)
            {
                var message = new
                {
                    to = user.Token,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = Message,
                        title = Title,
                        sound = "default"
                    },
                    data = new
                    {
                        id = "-1",
                        notificationtype = ((int)NotificationType.General).ToString(),
                    }
                };

                var Serializer = new JavaScriptSerializer();
                var Json = Serializer.Serialize(message);
                var byteArray = Encoding.UTF8.GetBytes(Json);

                try
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", IsDriver == true ? DriverServerKey : UserServerKey));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", IsDriver == true ? DriverSenderId : UserSenderId));
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    string sResponseFromServer = tReader.ReadToEnd();
                                    string str = sResponseFromServer;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                if (IsSaveInDatabase == true)
                {
                    SaveInDatabase(user.UserId, Title, Message, NotificationType.General, IsWorker: IsDriver);
                }
            }
        }

        public async static Task SendToAllSpecificIOSUserDevices(string UserId, string Title, string Message, NotificationType NotificationType, long Id = 0, bool IsDriver = false, bool IsSaveInDatabase = true)
        {
            var users = db.PushTokens.Where(d => d.OS == OS.IOS && d.UserId == UserId && d.IsWorker == IsDriver && d.IsDeleted == false).ToList();

            foreach (var user in users)
            {
                try
                {
                    var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, UserFullPath, certPassword);


                    // Create a new broker
                    var apnsBroker = new ApnsServiceBroker(config);

                    // Start the broker
                    apnsBroker.Start();

                    var Data = new
                    {
                        aps = new
                        {
                            alert = new
                            {
                                title = Title,
                                body = Message,
                            },
                            badge = 1,
                            sound = "default",
                        },
                        Id = Id,
                        NotificationType = (int)NotificationType
                    };

                    var modelToJson = JsonConvert.SerializeObject(Data);
                    var Payloadd = JObject.Parse(modelToJson);

                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = user.Token,
                        Payload = Payloadd,
                    });

                    // Stop the broker, wait for it to finish   
                    // This isn't done after every message, but after you're
                    // done with the broker
                    apnsBroker.Stop();

                }
                catch (Exception)
                {
                }
                
            }
            //if (IsSaveInDatabase == true)
            //{
            //    SaveInDatabase(UserId, Title, Message, NotificationType, Id, IsWorker: IsDriver);
            //}
        }

        public async static Task SendToAllIOSDevices(string Title, string Message, bool IsDriver = false, bool IsWorker = false, bool IsSaveInDatabase = true)
        {
            List<PushToken> users = new List<PushToken>();
            if (IsDriver == false && IsWorker == false)
            {
                users = db.PushTokens.Where(d => d.OS == OS.IOS && d.IsDeleted == false && d.IsWorker == false).ToList();
            }
            if (IsDriver == true)
            {
                users = db.PushTokens.Where(d => d.OS == OS.IOS && d.IsDeleted == false && d.IsWorker == true && d.User.Drivers.Any(w => w.IsDeleted == false) == true).ToList();
            }
            if (IsWorker == true)
            {
                users = db.PushTokens.Where(d => d.OS == OS.IOS && d.IsDeleted == false && d.IsWorker == true && d.User.Workers.Any(w => w.IsDeleted == false) == true).ToList();
            }

            foreach (var user in users)
            {
                try
                {
                    var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, UserFullPath, certPassword);


                    // Create a new broker
                    var apnsBroker = new ApnsServiceBroker(config);

                    // Start the broker
                    apnsBroker.Start();

                    var Data = new
                    {
                        aps = new
                        {
                            alert = new
                            {
                                title = Title,
                                body = Message,
                            },
                            badge = 1,
                            sound = "default",
                        },
                        Id = -1,
                        NotificationType = (int)NotificationType.General
                    };
                    var modelToJson = JsonConvert.SerializeObject(Data);
                    var Payloadd = JObject.Parse(modelToJson);

                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = user.Token,
                        Payload = Payloadd,

                    });


                    // Stop the broker, wait for it to finish   
                    // This isn't done after every message, but after you're
                    // done with the broker
                    apnsBroker.Stop();
                }
                catch (Exception)
                {
                }
                if (IsSaveInDatabase == true)
                {
                    SaveInDatabase(user.UserId, Title, Message, NotificationType.General, IsWorker: IsDriver);
                }
            }
        }
        public static void SaveInDatabase(string UserId, string Title, string Message, NotificationType NotificationType, long Id = -1, string WebLink = null, long? StoreId = -1, bool IsWorker = false)
        {
            try
            {
                var dbObject = db.Notifications.FirstOrDefault(s => s.IsDeleted == false && s.IsSeen == false && s.UserId == UserId && s.IsWorker == IsWorker && s.Title == Title && s.Body == Message);
                if (dbObject == null)
                {
                    // Id = -1 means that there is not ID, default value.
                    var Notification = new Notification()
                    {
                        NotificationType = NotificationType,
                        IsWorker = IsWorker,
                        Body = Message,
                        IsSeen = false,
                        Title = Title,
                        UserId = UserId,
                        RequestId = Id,
                        NotificationLink = WebLink
                    };
                    if (StoreId.HasValue == true && StoreId.Value > 0)
                    {
                        Notification.StoreId = StoreId;
                    }
                    db.Notifications.Add(Notification);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}