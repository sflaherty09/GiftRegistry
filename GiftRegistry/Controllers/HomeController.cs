/**/
/*
    Name:

        HomeController
    
    Purpose: 
        
        To handle all information being transferred between the Home Model and the Home Views.
        Each function acts differently depending on whether it is a GET or POST request. It's primary purpose 
        is to allow user to navigate the home page
    
    Author:
        Sean Flaherty
 */
/**/
using System.Web.Mvc;

namespace GiftRegistry.Controllers
{
    public class HomeController : Controller
    {

        /**/
        /*
                public Index()
                GET Request

        NAME

                Index - Display home page

        DESCRIPTION

                Shows us the application home page, which will be seen whenever the application starts

        RETURNS

               The Index View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        public ActionResult Index()
        {
            return View();
        }


        /**/
        /*
                public About()
                GET Request

        NAME

                About - Displays the about page for the app

        DESCRIPTION

                Shows us the about page, which is just where I explain what the app is
                in more detail

        RETURNS

               The About View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        public ActionResult About()
        {

            return View();
        }


        /**/
        /*
                public Contact()
                GET Request

        NAME

                Contact - Displays your contact information

        DESCRIPTION

                Shows Contact information

        RETURNS

               The Contact View

        AUTHOR

                Automatically Generated

        DATE

                1/30/18

        */
        /**/
        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}