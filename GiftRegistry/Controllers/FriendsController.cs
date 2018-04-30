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


        /**/
        /*
                public ActionResult Index(string searchString)
                GET Request

        NAME

                Index - Lets us see a list of all of our friends and their registry lists

        SYNOPSIS

                    public ActionResult Index(string searchString)
                    searchString             --> allows us to look up our friends by name

        DESCRIPTION

                The Index View goes through our Friend's list database, their entries in the GiftLists database
                and their entry in the App user database to get all their important information and display it in a 
                way that make sense

        RETURNS

               The Index View, with the possibility of refining it with searchString

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

        /**/
        /*
                public ActionResult AddFriends(string searchString)
                GET Request

        NAME

                AddFriends - Shows the user a list of all other users they are currently not friends with
                and gives them the option to add new friends, also you can use the searchstring to refine your
                search if you already know who you are looking for

        SYNOPSIS

                    public ActionResult AddFriends(string searchString)
                    searchString             --> allows us to look up our friends by name

        DESCRIPTION

                The AddFriends Veiw goes through both the Friends Database and the User Database
                and displays all the users who are not currently already friends with the current
                logged in user

        RETURNS

               The AddFriends View, with the possibility of refining it with searchString

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
        // GET: Friends/AddFriends
        [Authorize]
        [HttpGet]
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

        /**/
        /*
                public ActionResult SendFriendRequest(string id)
                GET Request

        NAME

                SendFriendRequest - Pulls up the user with the given id and confirms with the
                logged in user that they would indeed like to be friends with this given person,
                they are then given the option to send or decline

        SYNOPSIS

                    public ActionResult SendFriendRequest(string id)
                    id             --> the id of the user we want to send the request to

        DESCRIPTION

                Pulls up a page to confirm whether or not the user wants to be friends with this person,
                to ensure we are not accidentally sending emails to people we do not want to be friends with

        RETURNS

               SendFriendRequest View, with all of the user's information passed into it

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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
        /**/
        /*
                public ActionResult SendFriendRequest(string id)
                POST Request

        NAME

                SendFriendRequest - Sends an email notifying one user that another user
                wants to be their friend

        SYNOPSIS

                    public ActionResult SendFriendRequest(string id)
                    id             --> the id of the user we want to send the request to

        DESCRIPTION

                Finds the user's email whose id we pass and send them an email saying that a user
                wants to be their friend, with a link taking them to a page where they can accept
                or deny it 

        RETURNS

               A redirect back to the AddFriends page

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

            var callBackUrl = Url.Action("FriendRequest", "Friends", new { fromId = User.Identity.GetUserId() }, protocol: Request.Url.Scheme);

            string message = fromUser.Name + " would like to be your friend. Click <a href=\"" + callBackUrl + "\">here</a> to view";


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

        /**/
        /*
                public ActionResult FriendRequest(string fromId)
                GET Request

        NAME

                FriendRequest -  Shows a Friend request from one user to another, where 
                they can decide whether or not they would like to add a new friend

        SYNOPSIS

                    public ActionResult FriendRequest(string fromId)
                    fromId             --> the id of the user who sent you the request

        DESCRIPTION

                Pulls up a page giving the user the option as to whether or not they would like to add this friend
                giving them all that user's information

        RETURNS

               FriendRequest View, with all of the user's information passed into it,
               and the option to confirm or deny

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

        /**/
        /*
                public ActionResult FriendRequest(string fromId, string answer)
                POST Request

        NAME

                FriendRequest -  Confrims or denies the friend request sent

        SYNOPSIS

                    public ActionResult FriendRequest(string fromId, string answer)
                    fromId             --> the id of the user who sent you the request
                    answer             --> the result of the friend request

        DESCRIPTION

                Takes the answer of the Friend Request and either adds the proper informaton into
                the Friend database, which is both user ids and two notification flags set to false
                or sends a redirect to a different page

        RETURNS

               A redirect to Index, which is the list of all friends, regardless of the outcome

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

        /**/
        /*
                public ActionResult Delete(int id)
                GET Request

        NAME

                Delete -  Prompts user to ask them if they want to delete friend

        SYNOPSIS

                    public ActionResult Delete(int id)
                    id             --> the id of the database entry where this friendship exists

        DESCRIPTION

                Posts the information about the friend in question that you are saying that you want 
                to delete, prompting to you to make sure that you want to do it

        RETURNS

               A View with the friend that you want to delete's info

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

        /**/
        /*
                public ActionResult DeleteConfirmed(int id)
                POST Request

        NAME

                Delete -  Deletes this friendship if the user confirms

        SYNOPSIS

                    public ActionResult Delete(int id)
                    id             --> the id of the entry in the database that is being deleted

        DESCRIPTION

                Removes this friendship's info from the friends database

        RETURNS

               A redirect to the friend's list where you will see that this friend 
               is no longer there

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
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

        /**/
        /*
                public ActionResult RecommendToFriend()
                GET Request

        NAME

                RecommendToFriend -  Recommend a gift idea to a friend


        DESCRIPTION

                Pulls up a form, like how you would normally create an entry for the 
                gift list, but it allows you to put in information for a friend, giving
                them a suggestion of something they may like

        RETURNS

               A View with a form to fill out and send to your friend

        AUTHOR

                Sean Flaherty

        DATE

                4/21/18

        */
        /**/
        // GET: Friends/RecommendToFriend
        public ActionResult RecommendToFriend()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecommendToFriend([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList, string id)
        {
            var client = new SendGridClient("your api key");

            var callBackUrl = Url.Action("FriendRecommendation", "GiftLists", new { recommendation = giftList}, protocol: Request.Url.Scheme);

            string message =  "Someone has sent you a recommendation, click <a href=\"" + callBackUrl + "\">here</a> to see what it is!";

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("reminders@gifts.com", "GiftRegistry"),
                Subject = "Someone has sent you a recommendation!",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(usersDb.Users.Find(id).Email));
            client.SendEmailAsync(msg);

            return RedirectToAction("Index");
        }

        // CHECK BIRTHDAYS
        public void CheckBirthdays()
        {
            var appUsers = from u in usersDb.Users
                           select u;

            var friends = from f in db.FriendsModels
                          select f;

            foreach(var au in appUsers)
            {
                DateTime thisBirthday = new DateTime(DateTime.Now.Year, au.BirthDate.Month, au.BirthDate.Day);
                int diffWeeks = (int) (thisBirthday - DateTime.Now).TotalDays / 7;

                if (diffWeeks <= 2 && diffWeeks > 0)
                {
                    foreach (var f in friends)
                    {
                        if (f.FriendID1 == au.Id && !f.Friend2NotificationFlag)
                        {
                            // Send Email to f.FriendId2
                            f.Friend2NotificationFlag = true;

                            string sendTo = usersDb.Users.Find(f.FriendID2).Email;
                            string name = au.Name;

                            SendBirthdayReminder(sendTo, name, au.Id);
                        }
                        else if (f.FriendID2 == au.Id && !f.Friend1NotificationFlag)
                        {
                            // Send Email to f.FriendId1
                            f.Friend1NotificationFlag = true;

                            string sendTo = usersDb.Users.Find(f.FriendID1).Email;
                            string name = au.Name;

                            SendBirthdayReminder(sendTo, name, au.Id);
                        }
                    }
                }
                else if (diffWeeks < 0)
                {
                    foreach (var f in friends)
                    {
                        if (f.FriendID1 == au.Id && f.Friend2NotificationFlag)
                        {
                            f.Friend2NotificationFlag = false;
                        }
                        else if (f.FriendID2 == au.Id && f.Friend1NotificationFlag)
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

            var callBackUrl = Url.Action("PublicList", "GiftLists", new { userId = userID}, protocol: Request.Url.Scheme);

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
