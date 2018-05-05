using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using GiftRegistry.Models;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Controllers
{
    public class GiftListsController : Controller
    {
        private GiftRegistryContext m_giftDb = new GiftRegistryContext();

        private ApplicationDbContext m_userDb = new ApplicationDbContext();

        private FriendsContext m_friendsDb = new FriendsContext();

        /**/
        /*
                public ActionResult Index(string giftCategory, string searchString, string sortBy)
                GET Request

        NAME

                Index - Prints out currently logged in user's Gift List, while also providing an option to 
                search by name and category

        SYNOPSIS

                    public ActionResult Index(string giftCategory, string searchString, string sortBy)
                    giftCategory             --> string variable representing the different categories of gifts so we can sort by that
                    searchString             --> string variable that allows us to look up gifts by name
                    sortBy                   --> what we are sorting the gifts in the list by

        DESCRIPTION

                The Index view represent's the current User's gift list, so this controller function takes the search string varables that
                are being passed into it and looks in the GiftLists Model Db object and finds the appropriate data

        RETURNS

               The Index View with the appropriate data sorted through it

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        [Authorize]
        // GET: Index
        public ActionResult Index(string giftCategory, string searchString, string sortBy)
        {

            var categoryLst = new List<string>();

            var categoryQuery = from c in m_giftDb.GiftLists
                                orderby c.Category
                                select c.Category;

            categoryLst.AddRange(categoryQuery.Distinct());
            ViewBag.giftCategory = new SelectList(categoryLst);

            List<string> sortLst = new List<string>(new string[]
                {"Price Low-to-High", "Price High-to-Low", "Rating Low-to-High", "Rating High-to-Low" });
            ViewBag.sortBy = new SelectList(sortLst);

            var gifts = from g in m_giftDb.GiftLists
                        select g;

            if (!String.IsNullOrEmpty(searchString))
            {
                gifts = gifts.Where(s => s.GiftName.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(giftCategory))
            {
                gifts = gifts.Where(x => x.Category == giftCategory);
            }

            if (!String.IsNullOrEmpty(sortBy))
            {
                gifts = Sort(sortBy);
            }

            CheckForDeletions();

            return View(gifts.ToList());
        }

        /**/
        /*
                public ActionResult Details(int? id)
                GET Request

        NAME

                Details - Gives a more in depth view of a certain gift in the table

        SYNOPSIS

                    public ActionResult Details(int? id)
                    id             --> the id of the gift that we want to see in more detail

        DESCRIPTION

                The Details View gives us a more outlined and indepth view of what this entry in the table looks like
                We look in the database for the id of the gift that the user requested, and then return that into the view 

        RETURNS

               The Details View with a GiftList object representing one gift

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // GET: GiftLists/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = m_giftDb.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult Create()
                GET Request

        NAME

                Create - Pull up a form for user to add a gift to their registry


        DESCRIPTION

                Brings up the Create View so the user can add a gift to their registry
                upon completion we will go to the POST request method.

        RETURNS

               The Create View

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // GET: GiftLists/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }


        /**/
        /*
                public ActionResult Create([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList)
                POST Request

        NAME

                Create - Sends back to form data and sets the variables into an instance of GiftList Model

        SYNOPSIS

                    public ActionResult Create([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList)
                    Bind(Include = "") GiftList giftlist            --> the GiftList model being created and set

        DESCRIPTION

                Takes the form data and sets the variables into an instance of the GiftList Model and then 
                adds that instance into the GiftList database

        RETURNS

               The view with this instance set

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: GiftLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "ID,GiftName,Rating,Category,Price,Link")] GiftList giftList)
        {
            if (ModelState.IsValid)
            {
                giftList.UserId = User.Identity.GetUserId();
                giftList.Bought = false;
                m_giftDb.GiftLists.Add(giftList);
                m_giftDb.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(giftList);
        }

        /**/
        /*
                public ActionResult Edit(int? id)
                GET Request

        NAME

                Edit - Edit a user's gift registry entry using a form

        SYNOPSIS

                    public ActionResult Edit(int? id)
                    id          --> the id of the gift that we are looking to edit

        DESCRIPTION

                Takes the data from this entry in the gift registry, and fills it out to a form
                for the user to make any necessary changes they would like to it

        RETURNS

               The Edit View with the gift we want to change

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // GET: GiftLists/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = m_giftDb.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult Edit(int id)
                POST Request

        NAME

                Edit - Edit a user's gift registry entry using a form,
                then update the database

        SYNOPSIS

                    public ActionResult Edit(int? id)
                    id          --> the id of the gift that we are looking to edit

        DESCRIPTION

                Takes the data from this entry in the gift registry, and fills it out to a form
                for the user to make any necessary changes they would like to it, once posted it
                will validate this information and then update this entry in the database

        RETURNS

               The newly edited gift in the view

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: GiftLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Rating,Category,Price,Link")] GiftList giftList)
        {
            if (ModelState.IsValid)
            {
                m_giftDb.Entry(giftList).State = EntityState.Modified;
                m_giftDb.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult Delete(int? id)
                GET Request

        NAME

                Delete - Remove a gift from the user's registry

        SYNOPSIS

                    public ActionResult Delete(int? id)
                    id          --> the id of the gift that we are looking to delete

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys with the user that they indeed want to delete it

        RETURNS

               The Delete View with the info regarding this gift,
               so the user can decided if they want to delete it

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // GET: GiftLists/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = m_giftDb.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult DeleteConfirmed(int id)
                POST Request

        NAME

                DeleteConfirmed - Remove a gift from the user's registry

        SYNOPSIS

                    public ActionResult DeleteConfirmed(int id)
                    id          --> the id of the gift that we are looking to delete

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys with the user that they indeed want to delete it, upon
                answering yes, we will go into the database and remove this entry in their registry

        RETURNS

               A redirect to Index so they can see thier list with the entry they deleted now gone

        AUTHOR

                Sean Flaherty

        DATE

                1/30/18

        */
        /**/
        // POST: GiftLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            GiftList giftList = m_giftDb.GiftLists.Find(id);
            m_giftDb.GiftLists.Remove(giftList);
            m_giftDb.SaveChanges();
            return RedirectToAction("Index");
        }


        /**/
        /*
                public ActionResult PublicList(string userId, string sortBy, string giftCategory)
                GET Request

        NAME

                PublicList - Displays user's list for friends to see

        SYNOPSIS

                    public ActionResult PublicList(string userId, string sortBy, string giftCategory)
                    userId          --> the id of the user that we are displaying
                    sortBy          --> what we are sorting the gifts in the list by
                    giftCategory    --> if we are refining by category this is the category we will be using

        DESCRIPTION

                Using the userId passed into this function we go into the gift database and display
                all the gifts associated with this user, the difference with this one is that the user 
                in question cannot see it, as it is for their friends to see so they can determine
                what to buy them

        RETURNS

               The PublicList View with user object containing all their important information

        AUTHOR

                Sean Flaherty

        DATE

                3/20/18

        */
        /**/
        // GET: GiftLists/PublicList
        [Authorize]
        public ActionResult PublicList(string userId, string sortBy, string giftCategory)
        {
            var gifts = from g in m_giftDb.GiftLists
                        select g;

            var categoryLst = new List<string>();

            var categoryQuery = from c in m_giftDb.GiftLists
                                orderby c.Category
                                select c.Category;

            categoryLst.AddRange(categoryQuery.Distinct());
            ViewBag.giftCategory = new SelectList(categoryLst);

            List<String> sortLst = new List<string>(new string[]
            {"Price Low-to-High", "Price High-to-Low", "Rating Low-to-High", "Rating High-to-Low" });
            ViewBag.sortBy = new SelectList(sortLst);

            if (!String.IsNullOrEmpty(userId))
            {
                gifts = gifts.Where(s => s.UserId.Contains(userId));
            }

            if (!String.IsNullOrEmpty(giftCategory))
            {
                gifts = gifts.Where(x => x.Category == giftCategory);
            }
            if (!String.IsNullOrEmpty(sortBy))
            {
                gifts = Sort(sortBy);
            }

            UserModel users = new UserModel();
            users.Friends = null;
            users.Gifts = gifts.ToList();
            users.AppUser = m_userDb.Users.ToList();
            users.UserID = userId;

            return View(users);
        }

        /**/
        /*
                public ActionResult BuyGift(int? id)
                GET Request

        NAME

                BuyGift - Verify if a user wants to buy one of their friend's gifts

        SYNOPSIS

                    public ActionResult BuyGift(int? id)
                    id          --> the id of the gift that the user is looking to buy

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys that the user in fact wants to buy it

        RETURNS

               The BuyGift View with the info regarding this gift,
               so the user can decided if they want to buy it

        AUTHOR

                Sean Flaherty

        DATE

                3/30/18

        */
        /**/
        // GET: GiftLists/BuyGift/5
        [Authorize]
        public ActionResult BuyGift(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = m_giftDb.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult BuyGift(int id)
                POST Request

        NAME

                BuyGift - Verify if a user wants to buy one of their friend's gifts

        SYNOPSIS

                    public ActionResult BuyGift(int? id)
                    id          --> the id of the gift that the user is looking to buy

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys that the user in fact wants to buy it, once done the bought
                attribute is set to true, so this entry can eventually be deleted once the user's birthday 
                has passed

        RETURNS

               A redirect to this user's public page, so we can see that this gift has been bought

        AUTHOR

                Sean Flaherty

        DATE

                3/30/18

        */
        /**/
        //POST: GiftLists/BuyGift/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyGift(int id)
        {
            GiftList gift = m_giftDb.GiftLists.Find(id);
            gift.Bought = true;
            m_giftDb.SaveChanges();
            return RedirectToAction("PublicList");
        }


        /**/
        /*
                public ActionResult FriendRecommendation(int? id)
                POST Request

        NAME

                FriendRecommendation - View the gift that your friend recommended to you

        SYNOPSIS

                    public ActionResult FriendRecommendation(int? id)
                    id          --> the id of the gift that is being recommended to you

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys that the user in fact wants to add it to their list, if they decide 
                they do it will be added and they will be redirected to their list

        RETURNS

               The FriendRecommendation View with the recommended gift displayed

        AUTHOR

                Sean Flaherty

        DATE

                3/30/18

        */
        /**/
        //GET: GiftLists/FriendRecommendation
        [Authorize]
        public ActionResult FriendRecommendation(int? recommendationId)
        {
            if (recommendationId == null)
            {
                return View("Error");
            }

            GiftList giftList = m_giftDb.GiftLists.Find(recommendationId);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }


        /**/
        /*
                public ActionResult FriendRecommendation(int id)
                POST Request

        NAME

                FriendRecommendation - View the gift that your friend recommended to you

        SYNOPSIS

                    public ActionResult FriendRecommendation(int id)
                    id          --> the id of the gift that is being recommended to you

        DESCRIPTION

                Takes the data from this entry in the gift registry, pulls up all the information
                about this gift and verifys that the user in fact wants to add it to their list, if they decide 
                they do it will be added and they will be redirected to their list

        RETURNS

               Redirect to your list

        AUTHOR

                Sean Flaherty

        DATE

                3/30/18

        */
        /**/
        //POST: GiftLists/FriendRecommendation
        [Authorize]
        [HttpPost]
        public ActionResult FriendRecommendation(int recommendationId)
        {
            GiftList gift = m_giftDb.GiftLists.Find(recommendationId);

            gift.UserId = User.Identity.GetUserId();
            m_giftDb.SaveChanges();
            return RedirectToAction("Index");
        }


        /***************************************************************************************************
         * HELPER METHODS                                                                                  *
         * *************************************************************************************************/


        /**/
        /*
                public IQueryable<GiftList> Sort(string sortQuery)

        NAME

                Sort - Return the gift list in a sorted order determined by the user

        SYNOPSIS

                    public IQueryable<GiftList> Sort(string sortQuery)
                    sortQuery          --> how the user would like the gift list sorted

        DESCRIPTION

                Takes the query given by the user and sorts the table based on what they requested
                for example by price high-to-low or rating low-to-high

        RETURNS

               An organized gift list based on user request

        AUTHOR

                Sean Flaherty

        DATE

                4/23/18

        */
        /**/
        public IQueryable<GiftList> Sort(string sortQuery)
        {
            IQueryable<GiftList> refine;
            switch (sortQuery)
            {
                case "Rating High-to-Low":
                    refine = from r in m_giftDb.GiftLists
                             orderby r.Rating descending
                             select r;
                    return refine;
                case "Rating Low-to-High":
                    refine = from r in m_giftDb.GiftLists
                             orderby r.Rating ascending
                             select r;
                    return refine;
                case "Price High-to-Low":
                    refine = from r in m_giftDb.GiftLists
                             orderby r.Price descending
                             select r;
                    return refine;
                case "Price Low-to-High":
                    refine = from r in m_giftDb.GiftLists
                             orderby r.Price ascending
                             select r;
                    return refine;
                default:
                    var gifts = from g in m_giftDb.GiftLists
                                select g;
                    return gifts;
            }
        }


        /**/
        /*
                public void CheckForDeletions()

        NAME

                CheckForDeleteions - Checks to see if anything in the gift list can be deleted
                based on it being bought and the user's birhtday having passed 

        SYNOPSIS

                    public void CheckForDeletions()
                    nothing is passed into it

        DESCRIPTION

                Goes through the gift list databse and finds any entry where the gift has been 
                bought and the user in question's birthday has passed

        RETURNS

               Nothing, the function is void

        AUTHOR

                Sean Flaherty

        DATE

                4/23/18

        */
        /**/
        public void CheckForDeletions()
        {
            UserModel users = new UserModel();
            users.Friends = m_friendsDb.FriendsModels.ToList();
            users.Gifts = m_giftDb.GiftLists.ToList();
            users.AppUser = m_userDb.Users.ToList();


            foreach (var u in users.AppUser)
            {
                foreach (var g in users.Gifts)
                {
                    DateTime thisYearsBirthday = new DateTime(DateTime.Now.Year, u.BirthDate.Month, u.BirthDate.Day);
                    if (g.UserId == u.Id && thisYearsBirthday > DateTime.Now && g.Bought)
                    {
                        int giftId = g.ID;
                        GiftList giftList = m_giftDb.GiftLists.Find(giftId);
                        m_giftDb.GiftLists.Remove(giftList);
                        m_giftDb.SaveChanges();
                    }
                }
            }
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
                m_giftDb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
