using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using GiftRegistry.Models;

namespace GiftRegistry.Controllers
{
    public class FriendsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Friends
        public ActionResult Index()
        {
            return View();
        }

        // GET: AddFriend
        public ActionResult AddFriend()
        {
            var users = from u in db.Users
                        select u;

            return View(users.ToList());
        }

        /*
         *            var gifts = from g in db.GiftLists
                        select g;
         */
    }
}