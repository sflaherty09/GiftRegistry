/**/
/*
    Name:

        IdentityConfig
    
    Purpose: 
        
        To confirm user is who they say they are. Primary purpose is to send 
        verification emails so that not just anyone can make an account,
        they must have a valid email address first.  Also sends text messages
        for two factor authentication
    
    Author:
        Sean Flaherty
 */
/**/
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using GiftRegistry.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace GiftRegistry
{
    public class EmailService : IIdentityMessageService
    {

        /**/
        /*
                public async Task SendAsync(IdentityMessage message)

        NAME

                SendAsync - Send email that deals with confirming user's account

        SYNOPSIS

                    public async Task SendAsync(IdentityMessage message)
                    message                 --> the message to be set


        DESCRIPTION

                Recieves a message and then formats it and sends the email using SendGrid,
                used for confirming user's account

        RETURNS

               Nothing

        AUTHOR

                Sean Flaherty

        DATE

                2/15/18

        */
        /**/
        public async Task SendAsync(IdentityMessage message)
        {
            

             var client = new SendGridClient("You API Key");

            // Always delete the previous line and put this comment in instead or your account will get suspended 

            var credentials = new NetworkCredential(
           ConfigurationManager.AppSettings["mailAccount"],
           ConfigurationManager.AppSettings["mailPassword"]
           );

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("sflahert@gifts.com", "GiftRegistry"),
                Subject = message.Subject,
                PlainTextContent = message.Body,
                HtmlContent = "<strong>" + message.Body + "</strong>"
            };
            msg.AddTo(new EmailAddress(message.Destination));

            var response = await client.SendEmailAsync(msg);

        }
    }


    public class SmsService : IIdentityMessageService
    {

        /**/
        /*
                public async Task SendAsync(IdentityMessage message)

        NAME

                SendAsync - Send text message that deals with two factor autentication

        SYNOPSIS

                    public async Task SendAsync(IdentityMessage message)
                    message                 --> the message to be set


        DESCRIPTION

                Recieves a message and then formats it and sends the text message using Twilio,
                used for two factor autentication

        RETURNS

               Nothing

        AUTHOR

                Sean Flaherty

        DATE

                2/15/18

        */
        /**/
        public Task SendAsync(IdentityMessage message)
        {
            //string accountSid = ConfigurationManager.AppSettings["SMSAccountIdentification"];
            string accountSid = System.Configuration.ConfigurationManager.AppSettings["SMSAccountIdentification"];
            string authToken = System.Configuration.ConfigurationManager.AppSettings["SMSAccountPassword"];

            string fromNumber = ConfigurationManager.AppSettings["SMSAccountFrom"];

            // Initialize the Twilio client

            TwilioClient.Init(accountSid, authToken);

            MessageResource result = MessageResource.Create(

                    from: new PhoneNumber(fromNumber),

                    to: new PhoneNumber(message.Destination),

                    body: message.Body);


            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
