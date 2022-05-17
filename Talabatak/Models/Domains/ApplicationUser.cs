using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Talabatak.Models.Domains
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            CreatedOn = DateTime.Now.ToUniversalTime();
            Wallet = 0;
        }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int VerificationCode { get; set; }
        public string ForgotPasswordGUID { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal Wallet { get; set; }
        public long? CityId { get; set; }
        public virtual City City { get; set; }
        public RegisterationType RegisterationType { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string PaymentUniqueKey { get; set; }
        public decimal AmountToBePaid { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
        public virtual ICollection<StoreUser> Stores { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PushToken> PushTokens { get; set; }
        public virtual ICollection<Driver> Drivers { get; set; }
        public virtual ICollection<Worker> Workers { get; set; }
        public virtual ICollection<Complaint> Complaints { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<StoreOrder> StoreOrders { get; set; }
        public virtual ICollection<JobOrder> JobOrders { get; set; }
        public virtual ICollection<DriverReview> DriverReviews { get; set; }
        public virtual ICollection<StoreReview> StoreReviews { get; set; }
        public virtual ICollection<OtlobAy7agaOrder> OtlobAy7agaOrders { get; set; }
        public virtual ICollection<StoreOrderChat> SentStoreOrderChats { get; set; }
        public virtual ICollection<StoreOrderChat> RecievedStoreOrderChats { get; set; }
        public virtual ICollection<OtlobAy7agaOrderChat> SentOtlobAy7agaChats { get; set; }
        public virtual ICollection<OtlobAy7agaOrderChat> RecievedOtlobAy7agaChats { get; set; }
        public virtual ICollection<JobOrderChat> SentJobOrderChats { get; set; }
        public virtual ICollection<JobOrderChat> RecievedJobOrderChats { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
}