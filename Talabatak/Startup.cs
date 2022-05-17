using System;
using System.Collections.Generic;
using System.Linq;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Talabatak.Startup))]

namespace Talabatak
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRoles();
            CreateAdminUser();
           
            app.MapSignalR();
        }
       
        public void CreateRoles()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (!roleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
            }
            try
            {
                if (!roleManager.RoleExists("SubAdmin"))
                {
                    var role = new IdentityRole();
                    role.Name = "SubAdmin";
                    roleManager.Create(role);
                }
            }
            catch(Exception ex)
            {
                string exs = ex.Message;
                throw ex;
            }
            if (!roleManager.RoleExists("Store"))
            {
                var role = new IdentityRole();
                role.Name = "Store";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("Driver"))
            {
                var role = new IdentityRole();
                role.Name = "Driver";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("Worker"))
            {
                var role = new IdentityRole();
                role.Name = "Worker";
                roleManager.Create(role);
            }
        }
        public void CreateAdminUser()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Name = "Admin",
                Email = "admin@gmail.com",
                PhoneNumber = "01234567890",
                PhoneNumberConfirmed = true,
                UserName = "admin@gmail.com",
            };

            var bakup_user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Name = "Admin2",
                Email = "admin@gmail.com",
                PhoneNumber = "01000000000",
                PhoneNumberConfirmed = true,
                UserName = "admin@gmail.com",
            };
            string userPWD = "Admin@123";
            var IsExist = UserManager.FindByEmail(user.Email);
            if (IsExist == null)
            {
                var chkUser = UserManager.Create(user, userPWD);
                if (chkUser.Succeeded)
                {
                    var result = UserManager.AddToRole(user.Id, "Admin");
                }
            }

            var IsBakupExist = UserManager.FindByEmail(bakup_user.Email);
            if (IsBakupExist == null)
            {
                var chkUser = UserManager.Create(bakup_user, userPWD);
                if (chkUser.Succeeded)
                {
                    var result = UserManager.AddToRole(bakup_user.Id, "Admin");
                }
            }

        }
    }
}
