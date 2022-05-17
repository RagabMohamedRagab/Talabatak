using Talabatak.Models.Domains;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Collections.Generic;

namespace Talabatak.Models.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("TalabatakConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.SentStoreOrderChats).
                WithRequired(s => s.FromUser).
                HasForeignKey(s => s.FromUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.RecievedStoreOrderChats).
                WithRequired(s => s.ToUser).
                HasForeignKey(s => s.ToUserId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.SentOtlobAy7agaChats).
                WithRequired(s => s.FromUser).
                HasForeignKey(s => s.FromUserId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.RecievedOtlobAy7agaChats).
                WithRequired(s => s.ToUser).
                HasForeignKey(s => s.ToUserId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.SentJobOrderChats).
                WithRequired(s => s.FromUser).
                HasForeignKey(s => s.FromUserId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>().
                HasMany(w => w.RecievedJobOrderChats).
                WithRequired(s => s.ToUser).
                HasForeignKey(s => s.ToUserId).WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Category> Categories { get; set; }
      
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<PushToken> PushTokens { get; set; }
        public DbSet<SecondSlider> SecondSliders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreUser> StoreUsers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<UserFavoriteProduct> UserFavoriteProducts { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<MetaData> MetaDatas { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<StoreOrder> StoreOrders { get; set; }
        public DbSet<StoreOrderItem> StoreOrderItems { get; set; }
        public DbSet<StoreOrderDriver> StoreOrderDrivers { get; set; }
        public DbSet<DriverReview> DriverReviews { get; set; }
        public DbSet<StoreReview> StoreReviews { get; set; }
        public DbSet<OtlobAy7agaOrder> OtlobAy7agaOrders { get; set; }
        public DbSet<OtlobAy7agaOrderDriver> OtlobAy7agaOrderDrivers { get; set; }
        public DbSet<StoreOrderChat> StoreOrderChats { get; set; }
        public DbSet<OtlobAy7agaOrderChat> OtlobAy7agaOrderChats { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<MobileVersion> MobileVersions { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobOrder> JobOrders { get; set; }
        public DbSet<JobOrderChat> JobOrderChats { get; set; }
        public DbSet<JobOrderImage> JobOrderImages { get; set; }
        public DbSet<JobWorker> JobWorkers { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Slider> Slider { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; } 
     
    }
}