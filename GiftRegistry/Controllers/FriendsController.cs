/**/
/*
    Name:

        FriendsController
    
    Purpose: 
        
        To handle all information being transferred between the FriendsMdoel and the Friends Views.
        Each function acts differently depending on whether it is a GET or POST request. It's primary purpose 
        is to allow a user to add or delete friends, displays friends and their lists, and hanldes sending notifications
        to remind user's of upcoming birthdays
    
    Author:
        Sean Flaherty
 */
/**/
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using GiftRegistry.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNet.Identity;


namespace GiftRegistry.Controllers
{
    /**/
    /*
            public class FriendController : Controller

    NAME

            FriendController - Extends Controller object and handles all
            communication between the FriendsModel and the appropriate
            Friends Views

    DESCRIPTION

            Handles all communication between the Friends Model and the 
            Friends Views, so we are able to validate any changes to the Model,
            and organize data properly for the Views. 

    RETURNS

           Most methods direct the user to the correct views, with a few helper methods 
           on the bottom

    AUTHOR

            Sean Flaherty

    DATE

            4/15/18

    */
    /**/
    public class FriendsController : Controller
    {
        private FriendsContext m_friendsDb = new FriendsContext();

        private GiftRegistryContext m_giftDb = new GiftRegistryContext();

        private ApplicationDbContext m_usersDb = new ApplicationDbContext();


        /**************************************************************************************************
         * METHODS USED TO HANDLE MODEL-VIEW INTERACTIONS                                                 *
         **************************************************************************************************/

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
        [Authorize]
        public ActionResult Index(string searchString)
        {
            var appUsers = from u in m_usersDb.Users
                        select u;


            if (!String.IsNullOrEmpty(searchString))
            {
                appUsers = appUsers.Where(s => s.Name.Contains(searchString));
            }

            UserModel users = new UserModel();
            users.Friends = m_friendsDb.FriendsModels.ToList();
            users.Gifts = m_giftDb.GiftLists.ToList();
            users.AppUser = appUsers.ToList();
            CheckBirthdays();
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
            var appUsers = from u in m_usersDb.Users
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                appUsers = appUsers.Where(s => s.Name.Contains(searchString));
            }

            UserModel users = new UserModel();
            users.AppUser = appUsers.ToList();
            users.Friends = m_friendsDb.FriendsModels.ToList();
            users.Gifts = null;

            return View(users);
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
        [Authorize]
        public ActionResult SendFriendRequest(string id)
        {
            //var user = db3.Users.Find(id);


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var user = m_usersDb.Users.Find(id);

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
        [Authorize]
        public ActionResult SendFriendRequest(string id, string response)
        {
            var client = new SendGridClient("You API Key");

            // Always delete the previous line and put this comment in instead or your account will get suspended
            

            if (response == "No")
            {
                return RedirectToAction("AddFriend");
            }

            var user = m_usersDb.Users.Find(id);

            string friendEmail = user.Email;

            var fromUser = m_usersDb.Users.Find(User.Identity.GetUserId());

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


            return RedirectToAction("Index");
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
            var user = m_usersDb.Users.Find(fromId);
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
        [Authorize]
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

            m_friendsDb.FriendsModels.Add(friends);
            m_friendsDb.SaveChanges();

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
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FriendsModel friendsModel = m_friendsDb.FriendsModels.Find(id);
            if (friendsModel == null)
            {
                return HttpNotFound();
            }
            string friendId;
            if (friendsModel.FriendID1 == User.Identity.GetUserId())
            {
                friendId = friendsModel.FriendID2;
            }
            else if (friendsModel.FriendID2 == User.Identity.GetUserId())
            {
                friendId = friendsModel.FriendID1;
            }

            UserModel user = new UserModel();
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
            FriendsModel friendsModel = m_friendsDb.FriendsModels.Find(id);
            m_friendsDb.FriendsModels.Remove(friendsModel);
            m_friendsDb.SaveChanges();
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
        [Authorize]
        public ActionResult RecommendToFriend()
        {
            return View();
        }


        /**/
        /*
                public ActionResult RecommendToFriend([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList, string id)
                POST Request

        NAME

                RecommendToFriend -  Recommend a gift idea to a friend

        SYNOPSIS

                    public ActionResult RecommendToFriend([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList, string id)
                    id             --> the id of the user we are sending the recommendtation to
                    giftList       --> the gift you are recommending to your friend


        DESCRIPTION

                Upon verifying the data in the form we will add an entry into the gift's database,
                but set the user id to null, then send an email to the user in question informing them
                that someone has sent them a recommendation 

        RETURNS

               A redirect to your friends' page

        AUTHOR

                Sean Flaherty

        DATE

                4/21/18

        */
        /**/
        [HttpPost]
        [Authorize]
        public ActionResult RecommendToFriend([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList, string id)
        {

            giftList.UserId = null;
            giftList.Bought = false;
            m_giftDb.GiftLists.Add(giftList);
            m_giftDb.SaveChanges();
            int giftId = giftList.ID;
            

            var client = new SendGridClient("Your Api key");

            var callBackUrl = Url.Action("FriendRecommendation", "GiftLists", new { recommendationId = giftId}, protocol: Request.Url.Scheme);

            string message =  "Someone has sent you a recommendation, click <a href=\"" + callBackUrl + "\">here</a> to see what it is!";

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("reminders@gifts.com", "GiftRegistry"),
                Subject = "Someone has sent you a recommendation!",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(m_usersDb.Users.Find(id).Email));
            client.SendEmailAsync(msg);

            return RedirectToAction("Index");
        }

        /**************************************************************************************************
         * HELPER METHODS                                                                                 *
         **************************************************************************************************/

        /**/
        /*
                private void CheckBirthdays()

        NAME

                CheckBirthdays - Checks user database to see if there any upcoming
                birthdays


        DESCRIPTION

                Goes through the user database and checks to see if any user's birthdays
                are two weeks away, by comparing it to DateTime.Now. 
                If any are friend, it will then go through the friend database and notify
                all of that user's friends via email that their friend's birthday is coming up soon
                and they should check out their page to figure out what to get them

        RETURNS

               Nothing, it is a void function. It will redirect to SendBirthdayReminder
               if notification has to be sent

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
        private void CheckBirthdays()
        {
            var appUsers = from u in m_usersDb.Users
                           select u;

            var friends = from f in m_friendsDb.FriendsModels
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

                            string sendTo = m_usersDb.Users.Find(f.FriendID2).Email;
                            string name = au.Name;

                            SendBirthdayReminder(sendTo, name, au.Id);
                        }
                        else if (f.FriendID2 == au.Id && !f.Friend1NotificationFlag)
                        {
                            // Send Email to f.FriendId1
                            f.Friend1NotificationFlag = true;

                            string sendTo = m_usersDb.Users.Find(f.FriendID1).Email;
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
                else if (thisBirthday == DateTime.Now.Date)
                {
                    string sendTo = m_usersDb.Users.Find(au.Id).Email;
                    string name = au.Name;
                    SendHappyBirthday(sendTo, name);
                }
                m_friendsDb.SaveChanges();
            }
        }

        /**/
        /*
                private void SendBirthdayReminder(string emailAddress, string userName, string userID)

        NAME

                SendBirthdayReminder -  Send Email to user notifiying them that their friend's
                birthday is coming up

        SYNOPSIS

                    public void SendBirthdayReminder(string emailAddress, string userName, string userID)
                    emailAddress             --> the email address of the user we will be notifying
                    userName                 --> the name of the user we will be notifying
                    userID                   --> the id of the user who's list that you will be directed to 

        DESCRIPTION

                Sends an email to this user letting them know that one of their friend's birthdays is coming up.
                The email will include a link the their friend's gift list so they can easily view it and 
                go through to find out what they are getting them.

        RETURNS

               Nothing

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
        private void SendBirthdayReminder(string emailAddress, string userName, string userID)
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


        /**/
        /*
                private void SendHappyBirtday(string emailAddress, string userName)

        NAME

                SendHappyBirthday -  Send Happy Birthday messaage from app

        SYNOPSIS

                    public void SendBirthdayReminder(string emailAddress, string userName, string userID)
                    emailAddress             --> the email address of the user we will be saying happy birthday 
                    userName                 --> the name of the user we will be sending happy birthday  

        DESCRIPTION

                Sends an email to this user wishing them happy birthday 

        RETURNS

               Nothing

        AUTHOR

                Sean Flaherty

        DATE

                4/15/18

        */
        /**/
        private void SendHappyBirthday(string emailAddress, string userName)
        {
            var client = new SendGridClient("YOUR API KEY");
            


            string message = userName + ", Happy Birhday from GiftRegistry!!!";

            var msg = new SendGrid.Helpers.Mail.SendGridMessage()
            {
                From = new EmailAddress("birthday@gifts.com", "GiftRegistry"),
                Subject = "Happy Birthday!",
                PlainTextContent = message,
                HtmlContent = "<strong>" + message + "</strong>"
            };
            msg.AddTo(new EmailAddress(emailAddress));
            client.SendEmailAsync(msg);
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
            if (disposing)
            {
                m_friendsDb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
