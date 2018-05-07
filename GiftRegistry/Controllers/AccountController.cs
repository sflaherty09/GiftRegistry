/**/
/*
    Name:

        AccountController
    
    Purpose: 
        
        To handle all information being transferred between the AccountViewModels and the Account Views.
        Each function acts differently depending on whether it is a GET or POST request. It's primary purpose 
        is to allow a user to add account information, login, use two factor authentication, fix forgotten passwords etc.
        Information passed through here will update the model appopriately or help to display information to the View
    
    Author:
        Sean Flaherty
 */
/**/
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using GiftRegistry.Models;

namespace GiftRegistry.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        // Default Constructor 
        public AccountController()
        {
        }


        /**/
        /*
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )

        NAME

                AccountsController - Constructor for this Controller that takes a ApplicationUserManager and ApplicatoinSignInManager as arguments

        SYNOPSIS

                public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
                    ApplicationUserManager             --> object than handles user information
                    ApplicationSignInManager        --> object that handles loging in and registering 

        DESCRIPTION

                This constructor will assign this Controller's ApplicationUserManager and SignInManager's local variables that
                are being passed into it

        RETURNS

                Nothing, it is a constructor 

        AUTHOR

                Sean Flaherty, and partially created by Visual Studios upon creating a project

        DATE

                1/30/18

        */
        /**/
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        // Sets up the ApplicationSignInManager 
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        // Sets up the ApplicationUserManager
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /**/
        /*
                public ActionResult Login(string returnUrl)
                GET Request

        NAME

                Login - Shows in the log in form for the user to be able to login to the app

        SYNOPSIS

                    public ActionResult Login(string returnUrl)
                    returnUrl             --> where the user will go upon logging in 

        DESCRIPTION

                Shows a form for user to  enter in their password and username so they can 
                log in to app

        RETURNS

               The Login View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        /**/
        /*
                public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
                POST Request

        NAME

                Login - Verify user data is correct and that their email is verified so they 
                can log in and use the app

        SYNOPSIS

                    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
                    returnUrl             --> where the user will go upon logging in 
                    model                 --> the user that we are trying to verify

        DESCRIPTION

                Goes through user's information and makes sure that this username matches this password
                and that their email has been verified, if that is good then they can log in

        RETURNS

               Redirect to page they were trying to use if verified
               Back to login page if info was wrong
               Sends code if two facter authenticaion is necessary
               Or lockout page if they have attempted to log in too many times

        AUTHOR

                Automatically Generated and improved by Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                {
                    string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account-Resend");
                    ViewBag.errorMessage = "You must have a confirmed email to log on.";
                    return View("Error");
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }


        /**/
        /*
                public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
                GET Request

        NAME

                VerifyCode - Verify user identity using confirmation code that was sent to them over text

        SYNOPSIS

                    public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
                    returnUrl             --> where the user will go upon logging in 
                    provider              --> the provider of the service used to send the messages
                    rememberMe            --> whether or not the user wants this browser remembered so they can log in easier next time

        DESCRIPTION

                Sends a verification code then sends the user to a page where they can enter it to verify their 
                account through two factor authentication

        RETURNS

               The VerifyCode View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }


        /**/
        /*
                public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
                POST Request

        NAME

                VerifyCode - Verify user identity using confirmation code that was sent to them over text

        SYNOPSIS

                    public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
                    model             --> the model we are testing against

        DESCRIPTION

                Compare the code that was sent to the user with the one they entered if they are the same they can log
                in otherwise redirect them back to the page so they can enter it again 

        RETURNS

               Redirect to the page they were trying to go to if succeeded
               Lockout page if they tried too many times
               Back to the page they entered if they were wrong so they can try again

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }


        /**/
        /*
                public ActionResult Register()
                GET Request

        NAME

                Register - Add your user data into the application


        DESCRIPTION

                Shows a form for the user to add their information into, allowing them to add themselves to the 
                user database and be part of the application 

        RETURNS

               The Register View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        /**/
        /*
                public async Task<ActionResult> Register(RegisterViewModel model)
                POST Request

        NAME

                Register - Add user data into the database, and send verification email

        SYNOPSIS

                    public async Task<ActionResult> Register(RegisterViewModel model)
                    model                 --> the user that we are going to add

        DESCRIPTION

                Add all of user data into the model and send a verification email
                to the user in question so they can verify their email and use their account

        RETURNS

               It should return a page telling us to verify our email
               If it returns the original form then something has gone wrong

        AUTHOR

                Automatically Generated and improved by Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    BirthDate = model.BirthDate,
                    Name = model.Name
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //  Comment the following line to prevent log in until the user is confirmed.
                    //  await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                       new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                       "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    // Uncomment to debug locally 
                    // TempData["ViewBagLink"] = callbackUrl;

                    ViewBag.Message = "Check your email and confirm your account, you must be confirmed "
                                    + "before you can log in.";

                    return View("Info");
                    //return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);

        }


        /**/
        /*
                public async Task<ActionResult> ConfirmEmail(string userId, string code)
                GET Request

        NAME

                ConfirmEmail - Confirm user's email so they can use the application 

        SYNOPSIS

                    public async Task<ActionResult> ConfirmEmail(string userId, string code)
                    userId                 --> the user's id that needs email confirmed
                    code                   --> confirmation code

        DESCRIPTION

                Add all of user data into the model and send a verification email
                to the user in question so they can verify their email and use their account

        RETURNS

               It should return a page telling us to verify our email
               If it returns the original form then something has gone wrong

        AUTHOR

                Automatically Generated and improved by Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /**/
        /*
                public ActionResult ForgotPassword()
                GET Request

        NAME

                ForgotPassword - Brings user to a page where they can enter in their account info and
                get a new password 


        DESCRIPTION

                If user has forgotten password we will take them to a page where they can enter in their 
                account info and reset their password, this brings up the form allowing us to do that 

        RETURNS

               The ForgotPassword View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        /**/
        /*
                public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
                POST Request

        NAME

                ForgotPassword - Sends user an email with a link to reset their password

        SYNOPSIS

                    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
                    model                 --> the user who's password is forgotten


        DESCRIPTION

                Will send an email to the user who has forgotten their password, allowing them
                to come up with a new one

        RETURNS

               Should send user to confirmation password, will send back to form
               if something goes wrong

        AUTHOR

                Automatically Generated, improved by Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        /**/
        /*
                public ActionResult ForgotPasswordConfirmation()
                GET Request

        NAME

                ForgotPasswordConfirmation - Page that displays a confirmation that user's password change
                has been processed


        DESCRIPTION

                Displays a page notifying user that their password change request has been processed

        RETURNS

               ForgotPasswordConfirmation View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /**/
        /*
                public ActionResult ResetPassword(string code)
                GET Request

        NAME

                ResetPassword - Brings up a form for user to reset their password

        SYNOPSIS

                    public ActionResult ResetPassword(string code)
                    code                 --> password request confirmation code


        DESCRIPTION

                Form for user to reset their password is displayed

        RETURNS

               ResetPassword View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }


        /**/
        /*
                public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
                POST Request

        NAME

                ResetPassword - Processes user's password change

        SYNOPSIS

                    public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
                    model                 --> the user whose password we are gonna change


        DESCRIPTION

                Change the user's password as long as this user does in fact exist, 
                if not bring up the form and have them try again

        RETURNS

               The form if it was filled out incorrectly or the user doesn't exist
               Confirmation page if done correctly

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }


        /**/
        /*
                public ActionResult ResetPasswordConfirmation()
                GET Request

        NAME

                ResetPasswordConfirmation - Page that displays a confirmation that user's password has
                been changed

        DESCRIPTION

                Displays a page notifying user that their password has changed

        RETURNS

               ResetPasswordConfirmation View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }


        /**/
        /*
                public ActionResult LogOff()
                GET Request

        NAME

                LogOff - Log out current user

        DESCRIPTION

                Sign out current user from using the application 

        RETURNS

               Redirects user to homepage no matter where in the app they are

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
               "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return callbackUrl;
        }


        /**/
        /*
                protected override void Dispose(bool disposing)

        NAME

                Dispose -  Releases resources used, in this case
                the user manager and sign in manager

        SYNOPSIS

                    protected override void Dispose(bool disposing)
                    disposing             --> boolean value representing if the resources are being disposed

        DESCRIPTION

                Releases resources used by this class

        RETURNS

               Nothing

        AUTHOR

                Automatically generated

        DATE

                1/30/18

        */
        /**/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /**/
        /*
                private void AddErrors(IdentityResult result)

        NAME

                AddErrors -  Add errors to be reported to user 

        SYNOPSIS

                    private void AddErrors(IdentityResult result)
                    result             --> result of what the user did, will contain errors if something is wrong

        DESCRIPTION

                Adds errors made by user so they can see what they have done wrong trying to log in 

        RETURNS

               Nothing

        AUTHOR

                Automatically generated

        DATE

                1/30/18

        */
        /**/
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        /**/
        /*
                private ActionResult RedirectToLocal(string returnUrl)

        NAME

                RedirectToLocal -  Redirect user to a different page

        SYNOPSIS

                    private ActionResult RedirectToLocal(string returnUrl)
                    returnUrl             -->  the url the user is trying to go to

        DESCRIPTION

                Redirects user to the page they were trying to access

        RETURNS

               A redirect to the location user was trying to get to

        AUTHOR

                Automatically generated

        DATE

                1/30/18

        */
        /**/
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }


        }
        #endregion
    }
}