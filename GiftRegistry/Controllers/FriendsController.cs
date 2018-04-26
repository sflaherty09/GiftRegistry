using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GiftRegistry.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Controllers
{
    public class FriendsController : Controller
    {
        private FriendsContext db = new FriendsContext();

        private GiftRegistryContext giftDb = new GiftRegistryContext();

        private ApplicationDbContext usersDb = new ApplicationDbContext();


        // GET: Friends
        public ActionResult Index()
        {
            var appUsers = from u in usersDb.Users
                        select u;

            UserModel users = new UserModel();
            users.Friends = db.FriendsModels.ToList();
            users.Gifts = giftDb.GiftLists.ToList();
            users.AppUser = appUsers.ToList();
            return View(users);
        }

        // GET: Friends/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FriendsModel friendsModel = db.FriendsModels.Find(id);
            if (friendsModel == null)
            {
                return HttpNotFound();
            }
            return View(friendsModel);
        }

        // GET: Friends/Create
        [Authorize]
        public ActionResult AddFriends()
        {
            var users = from u in usersDb.Users
                        select u;

            return View(users.ToList());
        }

        [HttpGet]
        public ActionResult SendFriendRequest(string id)
        {
            //var user = db3.Users.Find(id);


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var user = usersDb.Users.Find(id);

            //ApplicationUser au = db.Users.Find(id);


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: SendFriendRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendFriendRequest(string id, string response)
        {
             var client = new SendGridClient("You API Key");

            //Always delete the previous line and put this comment in instead or your account will get suspended

            if (response == "No")
            {
                return RedirectToAction("AddFriend");
            }

            var user = usersDb.Users.Find(id);

            string friendEmail = user.Email;

            var fromUser = usersDb.Users.Find(User.Identity.GetUserId());

            //var callBackUrl = Url.Action("ConfirmRequest", "Friends", new { friendId = user.Id }, protocol: Request.Url.Scheme);

            var callBackUrl = Url.Action("FriendRequest", "Friends", new { fromId = User.Identity.GetUserId() }, protocol: Request.Url.Scheme);

            string message = fromUser.Name + " would like to be your friend. Click <a href=\"" + callBackUrl + "\">here</a> to view";


            // TEST THIS and create a new View for confirm friend request

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("friends@gifts.com", fromUser.Name),
                Subject = fromUser.Name + " has sent you a friend request",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(friendEmail));

            client.SendEmailAsync(msg);


            return RedirectToAction("AddFriends");
        }
         
        // ADD VIEW
        // GET: /FriendRequest
        [Authorize]
        public ActionResult FriendRequest(string fromId)
        {
            if (fromId == null)
            {
                return View("Error");
            }
            var user = usersDb.Users.Find(fromId);
            return View(user);
        }

        // POST: /FriendRequest
        [HttpPost]
        public ActionResult FriendRequest(string fromId, string answer)
        { 
            if (answer == "Deny")
            {
                return RedirectToAction("Index");
            }

            if (fromId == null)
            {
                return View("Error");
            }

            FriendsModel friends = new FriendsModel();

            friends.FriendID1 = User.Identity.GetUserId();
            friends.FriendID2 = fromId;
            friends.Friend1NotificationFlag = false;
            friends.Friend2NotificationFlag = false;

            db.FriendsModels.Add(friends);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Friends/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FriendsModel friendsModel = db.FriendsModels.Find(id);
            if (friendsModel == null)
            {
                return HttpNotFound();
            }
            return View(friendsModel);
        }

        // POST: Friends/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FriendsModel friendsModel = db.FriendsModels.Find(id);
            db.FriendsModels.Remove(friendsModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
