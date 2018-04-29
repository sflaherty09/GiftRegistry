using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GiftRegistry.Models;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Controllers
{
    public class GiftListsController : Controller
    {
        private GiftRegistryContext db = new GiftRegistryContext();

        private ApplicationDbContext userDb = new ApplicationDbContext();

        /**/
        /*
                public ActionResult Index(string giftCategory, string searchString)
                GET Request

        NAME

                Index - Prints out currently logged in user's Gift List, while also providing an option to 
                search by name and category

        SYNOPSIS

                    public ActionResult Index(string giftCategory, string searchString)
                    giftCategory             --> string variable representing the different categories of gifts so we can sort by that
                    searchString        --> string variable that allows us to look up gifts by name

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
        public ActionResult Index(string giftCategory, string searchString)
        {

            IQueryable<string> categoryQuery = from g in db.GiftLists
                                               orderby g.Category
                                               select g.Category;

            var gifts = from g in db.GiftLists
                        select g;

            if (!String.IsNullOrEmpty(searchString))
            {
                gifts = gifts.Where(s => s.GiftName.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(giftCategory))
            {
                gifts = gifts.Where(x => x.Category == giftCategory);
            }

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
            GiftList giftList = db.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }

        // GET: GiftLists/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

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
                db.GiftLists.Add(giftList);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(giftList);
        }

        // GET: GiftLists/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = db.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }

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
                db.Entry(giftList).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(giftList);
        }

        // GET: GiftLists/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = db.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }

        // POST: GiftLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            GiftList giftList = db.GiftLists.Find(id);
            db.GiftLists.Remove(giftList);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: GiftLists/BuyGift/5
        [Authorize]
        public ActionResult BuyGift(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiftList giftList = db.GiftLists.Find(id);
            if (giftList == null)
            {
                return HttpNotFound();
            }
            return View(giftList);
        }

        //POST: GiftLists/BuyGift/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyGift(int id)
        {
            GiftList gift = db.GiftLists.Find(id);
            gift.Bought = true;
            db.SaveChanges();
            return RedirectToAction("PublicList");
        }

        // GET: GiftLists/PublicList
        [Authorize]
        public ActionResult PublicList(string userId)
        {
            var gifts = from g in db.GiftLists
                        select g;

            if (!String.IsNullOrEmpty(userId))
            {
                gifts = gifts.Where(s => s.UserId.Contains(userId));
            }

            UserModel users = new UserModel();
            users.Friends = null;
            users.Gifts = db.GiftLists.ToList();
            users.AppUser = userDb.Users.ToList();
            users.UserID = userId;
            //GiftList giftList = db.GiftLists.Find(userId);
            return View(users);
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
