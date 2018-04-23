using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using GiftRegistry.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net;
using GiftRegistry.Controllers;

using SendGrid.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GiftRegistry.Controllers
{
    public class FriendsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        private IdentityDbContext db3 = new IdentityDbContext();

        private ApplicationUser db2 = new ApplicationUser();

        private ApplicationUserManager UserManager;


        // GET: Friends
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        // GET: AddFriend
        [Authorize]
        public ActionResult AddFriend()
        {
            var users = from u in db.Users
                        select u;

            return View(users.ToList());
        }

        [HttpGet]
        public ActionResult ConfirmRequest(string id)
        {
            //var user = db3.Users.Find(id);

            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            

            var user = db.Users.Find(id);

            //ApplicationUser au = db.Users.Find(id);


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: ConfirmRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmRequest(string id, string formValue)
        {

            if (formValue == "No")
            {
                return RedirectToAction("AddFriend");
            }

            var client = new SendGridClient("You API Key");

            // Always delete the previous line and put this comment in instead or your account will get suspended

            var user = db.Users.Find(id);

            string friendEmail = user.Email;

            var fromUser = db.Users.Find(User.Identity.GetUserId());

            string message = fromUser.Name + " would like to be your friend. Click HERE to accept";


            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("friends@gifts.com", fromUser.Name),
                Subject = "Friend Request",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(friendEmail));

            client.SendEmailAsync(msg);


            return RedirectToAction("AddFriend");
        }
    }
}