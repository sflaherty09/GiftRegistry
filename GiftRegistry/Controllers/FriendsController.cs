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
    /// <summary>
    /// Controller used to interact between Friends Model and the Views for Friends
    /// </summary>
    public class FriendsController : Controller
    {
        private FriendsContext db = new FriendsContext();

        private GiftRegistryContext giftDb = new GiftRegistryContext();

        private ApplicationDbContext usersDb = new ApplicationDbContext();


        /// <summary>
        /// GET: Friends
        /// What happens when you issue a GET Request on the friend's view
        /// Create a User Model so we can have all the information needed to display Friend information and their respective lists
        /// </summary>
        /// <returns>
        /// users -> A completed object with 3 lists
        /// 1. A list of friends
        /// 2. A list of user information
        /// 3. Their GiftList
        /// </returns>
        public ActionResult Index(string searchString)
        {
            var appUsers = from u in usersDb.Users
                        select u;


            if (!String.IsNullOrEmpty(searchString))
            {
                appUsers = appUsers.Where(s => s.Name.Contains(searchString));
            }

            UserModel users = new UserModel();
            users.Friends = db.FriendsModels.ToList();
            users.Gifts = giftDb.GiftLists.ToList();
            users.AppUser = appUsers.ToList();
            //CheckBirthdays();
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
        public ActionResult AddFriends(string searchString)
        {
            var appUsers = from u in usersDb.Users
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                appUsers = appUsers.Where(s => s.Name.Contains(searchString));
            }

            return View(appUsers.ToList());
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

            // Always delete the previous line and put this comment in instead or your account will get suspended

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

        // CHECK BIRTHDAYS
        public void CheckBirthdays()
        {
            var appUsers = from u in usersDb.Users
                           select u;

            var friends = from f in db.FriendsModels
                          select f;

            foreach(var x in appUsers)
            {
                DateTime thisBirthday = new DateTime();
                DateTime testing = new DateTime();
                if (x.BirthDate.Year <= DateTime.Now.Year)
                {
                    thisBirthday = x.BirthDate.AddYears(DateTime.Now.Year - x.BirthDate.Year);
                    testing = thisBirthday.AddDays(-14);
                }
                else
                {
                    testing = x.BirthDate.AddDays(-14);
                }
                if (DateTime.Now >= testing)
                {
                    // do stuff
                    foreach (var f in friends)
                    {
                        if (f.FriendID1 == x.Id && !f.Friend2NotificationFlag)
                        {
                            // Send Email to f.FriendId2
                            f.Friend2NotificationFlag = true;

                            string sendTo = usersDb.Users.Find(f.FriendID2).Email;
                            string name = x.Name;

                            SendBirthdayReminder(sendTo, name, x.Id);
                        }
                        else if (f.FriendID2 == x.Id && !f.Friend1NotificationFlag)
                        {
                            // Send Email to f.FriendId1
                            f.Friend1NotificationFlag = true;

                            string sendTo = usersDb.Users.Find(f.FriendID1).Email;
                            string name = x.Name;

                            SendBirthdayReminder(sendTo, name, x.Id);
                        }
                    }
                }
                else
                {
                    if (testing.Year == DateTime.Now.Year)
                    {
                        testing.AddYears(1);
                    }
                    foreach (var f in friends)
                    {
                        if (f.FriendID1 == x.Id && f.Friend2NotificationFlag && testing.Year > DateTime.Now.Year)
                        {
                            f.Friend2NotificationFlag = false;
                        }
                        else if (f.FriendID2 == x.Id && f.Friend1NotificationFlag && testing.Year > DateTime.Now.Year)
                        {
                            f.Friend1NotificationFlag = false;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public void SendBirthdayReminder(string emailAddress, string userName, string userID)
        {
            var client = new SendGridClient("YOUR API KEY");

            var callBackUrl = Url.Action("PublicList", "GiftLists", new { userId = userID }, protocol: Request.Url.Scheme);

            string message = userName + " has a birthday coming up, visit their <a href=\"" + callBackUrl  +"\">list</a> so you can order something!";

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("reminders@gifts.com", "GiftRegistry"),
                Subject = userName + "'s birthday is coming up!",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(emailAddress));
            client.SendEmailAsync(msg);

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
