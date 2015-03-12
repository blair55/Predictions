using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
//using Predictions.MvcOwin.Models;

namespace Predictions.MvcOwin
{
    //public class EmailService : IIdentityMessageService
    //{
    //    public Task SendAsync(IdentityMessage message)
    //    {
    //        // Plug in your email service here to send an email.
    //        return Task.FromResult(0);
    //    }
    //}

    //public class SmsService : IIdentityMessageService
    //{
    //    public Task SendAsync(IdentityMessage message)
    //    {
    //        // Plug in your SMS service here to send a text message.
    //        return Task.FromResult(0);
    //    }
    //}

    //// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    //public class ApplicationUserManager : UserManager<ApplicationUser>
    //{
    //    public ApplicationUserManager(IUserStore<ApplicationUser> store)
    //        : base(store)
    //    {
    //    }

    //    public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
    //    {
    //        var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
    //        // Configure validation logic for usernames
    //        manager.UserValidator = new UserValidator<ApplicationUser>(manager)
    //        {
    //            AllowOnlyAlphanumericUserNames = false,
    //            RequireUniqueEmail = true
    //        };

    //        // Configure validation logic for passwords
    //        manager.PasswordValidator = new PasswordValidator
    //        {
    //            RequiredLength = 6,
    //            RequireNonLetterOrDigit = true,
    //            RequireDigit = true,
    //            RequireLowercase = true,
    //            RequireUppercase = true,
    //        };

    //        // Configure user lockout defaults
    //        manager.UserLockoutEnabledByDefault = true;
    //        manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //        manager.MaxFailedAccessAttemptsBeforeLockout = 5;

    //        // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
    //        // You can write your own provider and plug it in here.
    //        manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
    //        {
    //            MessageFormat = "Your security code is {0}"
    //        });
    //        manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
    //        {
    //            Subject = "Security Code",
    //            BodyFormat = "Your security code is {0}"
    //        });
    //        manager.EmailService = new EmailService();
    //        manager.SmsService = new SmsService();
    //        var dataProtectionProvider = options.DataProtectionProvider;
    //        if (dataProtectionProvider != null)
    //        {
    //            manager.UserTokenProvider = 
    //                new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
    //        }
    //        return manager;
    //    }
    //}

    //// Configure the application sign-in manager which is used in this application.
    //public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    //{
    //    public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
    //        : base(userManager, authenticationManager)
    //    {
    //    }

    //    public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
    //    {
    //        return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
    //    }

    //    public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
    //    {
    //        return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
    //    }
    //}

        
    public class PlUser : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<PlUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    return await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //}
    }

    public class PlUserStore : IUserStore<PlUser>
    {
        public async Task<PlUser> FindByNameAsync(string name) { throw new Exception(); }
        public async Task<PlUser> FindByIdAsync(string name) { throw new Exception(); }
        public async Task UpdateAsync(PlUser user) { throw new Exception(); }
        public async Task DeleteAsync(PlUser user) { throw new Exception(); }
        public async Task CreateAsync(PlUser user) { throw new Exception(); }

        public void Dispose()
        {
            this.Dispose();
        }
    }

    public class PlUserManager : UserManager<PlUser>
    {
        public PlUserManager(IUserStore<PlUser> store) : base(store)
        {}
    }

    public class PlSignInManager : SignInManager<PlUser, string>
    {
        public PlSignInManager(PlUserManager userManager, IAuthenticationManager authenticationManager) : base(userManager, authenticationManager)
        {}

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(PlUser user)
        //{
        //    return user.GenerateUserIdentityAsync((PlUserManager)UserManager);
        //}

        //public static PlSignInManager Create(IdentityFactoryOptions<PlSignInManager> options, IOwinContext context)
        //{
        //    return new PlSignInManager(context.GetUserManager<PlUserManager>(), context.Authentication);
        //}
    }
}
