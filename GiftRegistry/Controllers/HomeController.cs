using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GiftRegistry.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace GiftRegistry.Controllers
{
    public class HomeController : Controller
    {
        private FriendsContext db = new FriendsContext();

        private GiftRegistryContext giftDb = new GiftRegistryContext();

        private ApplicationDbContext usersDb = new ApplicationDbContext();

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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AppHome()
        {
            return View();
        }

        public ActionResult About()
        {
            string message = Extract();

            if (message == null || message == "")
            {
                ViewBag.Message = "Can't Find";
            }
            else
            {
                ViewBag.Message = message;
            }

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public string Extract()
        {
            var html = new HtmlDocument();
            //html.Load(@"C:\Users\Owner\Documents\Visual Studio 2015\Projects\GiftRegistry\GiftRegistry\Views\Home\Temporary.cshtml");

            /*
            html.LoadHtml(new WebClient().DownloadString("https://www.ae.com/men-jeans-ae-extreme-flex-super-skinny-jean-dark-wash/web/s-prod/1112_4123_896?cm=sUS-cUSD&catId=cat5850028"));

            var root = html.DocumentNode;
            var p = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals("psp-product-saleprice")).First();


            var content = p.InnerText;

            var d = html.DocumentNode.Descendants("title").FirstOrDefault();

            string title = d.InnerHtml;
            */
            string title;

            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString("https://www.ae.com/men-jeans-ae-extreme-flex-super-skinny-jean-dark-wash/web/s-prod/1112_4123_896?cm=sUS-cUSD&catId=cat5850028");
                htmlCode = htmlCode.Substring(
                    htmlCode.IndexOf("$")
                    );
                title = Regex.Match(htmlCode, @"\d+\d+").Value;
            }


            //var x = Regex.Match(content, @"\d+\d+").Value;
            return title;
        }
    }
}