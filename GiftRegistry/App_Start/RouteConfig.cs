/**/
/*
    Name:

        RouteConfig
    
    Purpose: 
        
        Handles where how the user traverses through pages in the app
    
    Author:
        Automatically Generated
 */
/**/
using System.Web.Mvc;
using System.Web.Routing;

namespace GiftRegistry
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
