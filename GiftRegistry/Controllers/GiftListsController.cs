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

        [Authorize]
        // GET: GiftLists
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

            //var giftCategoryVM = new GiftListsCategoryViewModel();
            //giftCategoryVM.categories = new SelectList(await categoryQuery.Distinct().ToListAsync());
            //giftCategoryVM.gifts = await gifts.ToListAsync();

            //return View(giftCategoryVM);

            return View(gifts.ToList());
        }

        // GET: GiftLists/Details/5
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: GiftLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public ActionResult DeleteConfirmed(int id)
        {
            GiftList giftList = db.GiftLists.Find(id);
            db.GiftLists.Remove(giftList);
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
