/**/
/*
    Name:

        ManageController
    
    Purpose: 
        
        To handle all information being transferred between the Manage Model and the Manage Views.
        Each function acts differently depending on whether it is a GET or POST request. It's primary purpose 
        is to allow a user to optimize their account how they would like it, like adding phone numbers setting up
        two factor authenticaion, changing password etc.
    
    Author:
        Sean Flaherty
 */
/**/
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
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        // Default Constructor
        public ManageController()
        {
        }

        // Constructor taking a user namanger and signin manager as agruments
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        // Sets up the application sign in manager
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

        // Sets up the application user manager
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
                public async Task<ActionResult> Index(ManageMessageId? message)
                GET Request

        NAME

                Index - Your management page, giving you the ability to add details to your account

        SYNOPSIS

                    public async Task<ActionResult> Index(ManageMessageId? message)
                    message                 --> the message for the user


        DESCRIPTION

                Displays your account information giving you the option to change things about your account
                such as addinga phone number or changing your password

        RETURNS

               The Index View for account management

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }


        /**/
        /*
                public ActionResult AddPhoneNumber()
                GET Request

        NAME

                AddPhoneNumber - Add a phone number to your account


        DESCRIPTION

                Pulls up a form for the user to add a phone number

        RETURNS

               The AddPhoneNumber View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }


        /**/
        /*
                public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
                POST Request

        NAME

                AddPhoneNumber - Add a phone number to your account

        SYNOPSIS

                    public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
                    model                 --> the model that handles adding a phone number


        DESCRIPTION

                Verifies that you entered in a valid phone number and confirms your account
                by sending you a confirmation code

        RETURNS

               A redirect to VerifyPhoneNumber to verify that you got the code

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }


        /**/
        /*
                public async Task<ActionResult> EnableTwoFactorAuthentication()
                POST Request

        NAME

                EnableTwoFactorAuthentication - Turns on two factor authentication


        DESCRIPTION

                Sets two factor authentication value in the table to true, and will 
                send users a text message every time they want to log in

        RETURNS

               A redirect to the Index page

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /**/
        /*
                public async Task<ActionResult> DisableTwoFactorAuthentication()
                POST Request

        NAME

                DisableTwoFactorAuthentication - Turns off two factor authentication


        DESCRIPTION

                Sets two factor authentication value in the table to false, and will 
                no longer send users a text message every time they want to log in

        RETURNS

               A redirect to the Index page

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        /**/
        /*
                public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
                GET Request

        NAME

                VerifyPhoneNumber - verifies that the user entered in a valid phone number and sends
                them a confirmation code

        SYNOPSIS

                    public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
                    phoneNumber                 --> the user's phone number that we are testing 


        DESCRIPTION

                Sends the user a confirmation code that they enter into the form verifying that 
                they entered in the right phone number

        RETURNS

               Whether or not this phone number is valid

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }


        /**/
        /*
                public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
                POST Request

        NAME

                VerifyPhoneNumber - checks the code the user entered with the one that was sent

        SYNOPSIS

                    public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
                    model                 --> the model that will help us verify the phone number 


        DESCRIPTION

                Tests the code the user entered against what was sent to them and if they are teh same 
                it will confirm this phone number as valid 

        RETURNS

               A success message if valid or a fail if invalid

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }


        /**/
        /*
                public async Task<ActionResult> RemovePhoneNumber()
                POST Request

        NAME

                RemovePhoneNumber - remove a phone number from your account 


        DESCRIPTION

                Removes phone number from your account 

        RETURNS

               A redirect to Index View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }


        /**/
        /*
                public ActionResult ChangePassword()
                GET Request

        NAME

                ChangePassword - change user's password


        DESCRIPTION

                Pulls up a form for user to change their password 

        RETURNS

               The ChangePassword View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }


        /**/
        /*
                public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
                POST Request

        NAME

                ChangePassword - change user's password

        SYNOPSIS

                    public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
                    model                 --> the model that will help us change the user's password


        DESCRIPTION

                Changes the user's password in the table

        RETURNS

                Redirect to Index View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }


        /**/
        /*
                public ActionResult DeleteAccount()
                GET Request

        NAME

                DeleteAccount - Give user opportunity to delete their account

        DESCRIPTION

                Gives the user a prompt asking them if they want to delete their account

        RETURNS

               DeleteAccount View

        AUTHOR

                Sean Flaherty

        DATE

                2/15/18

        */
        /**/
        // GET: /Manage/DeleteAccount
        [Authorize]
        public ActionResult DeleteAccount()
        {
            return View();
        }

        /**/
        /*
                public ActionResult DeleteAccountConfirmed(string id)
                GET Request

        NAME

                DeleteAcountConfirmed - Deletes user from app, as well as every reference to them

        SYNOPSIS

                    public ActionResult DeleteAccountConfirmed(string id)
                    id                 --> the id of the user getting deleted


        DESCRIPTION

                Goes through both the friends and gifts database and finds any reference
                to this user's id and deletes any information regarding them
                as to not keep useless data lying around

        RETURNS

               Redirect to a different page

        AUTHOR

                Sean Flaherty

        DATE

                2/15/18

        */
        /**/
        // GET: /Manage/DeleteAccountConfirmed
        public ActionResult DeleteAccountConfirmed(string id)
        {
            FriendsContext friendsDb = new FriendsContext();

            GiftRegistryContext giftDb = new GiftRegistryContext();

            ApplicationDbContext usersDb = new ApplicationDbContext();

            foreach (var friend in friendsDb.FriendsModels)
            {
                if (friend.FriendID1 == id || friend.FriendID2 == id)
                {
                    friendsDb.FriendsModels.Remove(friendsDb.FriendsModels.Find(friend.ID));
                }
            }
            friendsDb.SaveChanges();

            foreach (var gift in giftDb.GiftLists)
            {
                if (gift.UserId == id)
                {
                    giftDb.GiftLists.Remove(giftDb.GiftLists.Find(gift.ID));
                }
            }
            giftDb.SaveChanges();

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            usersDb.Users.Remove(usersDb.Users.Find(id));

            usersDb.SaveChanges();

            return View();
        }


        /**/
        /*
                protected override void Dispose(bool disposing)

        NAME

                Dispose -  Releases resources used, in this case
                the databases

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

                4/5/18

        */
        /**/
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        // The authenticaion manager is set here so we can check to see who is logged in
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

                AddErrors -  Adds errors to be reported to user

        SYNOPSIS

                    private void AddErrors(IdentityResult result)
                    result             --> object that will contain an errors that 
                    may have occured while trying to log in

        DESCRIPTION

                Adds errors to be reported to user

        RETURNS

               Nothing

        AUTHOR

                Automatically generated

        DATE

                4/5/18

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
                private bool HasPassword()

        NAME

                HasPassword -  Returns whether or not user has password


        DESCRIPTION

                Checks to see if user has password or not, ensuring that they are able to log in

        RETURNS

               True if they have password false otherwise

        AUTHOR

                Automatically generated

        DATE

                1/30/18

        */
        /**/
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }


        /**/
        /*
                private bool HasPhoneNumber()

        NAME

                HasPhoneNumber -  Returns whether or not user has phone number added


        DESCRIPTION

                Checks to see if user has phone number or not, ensuring that they are able to log in
                using two factor authentication

        RETURNS

               True if they have a phone number false otherwise

        AUTHOR

                Automatically generated

        DATE

                1/30/18

        */
        /**/
        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}