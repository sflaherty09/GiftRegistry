/**/
/*
    Name:

        IdentityModel
    
    Purpose: 
        
        To handle all information regarding a user's personal data.
    
    Author:
        Sean Flaherty
 */
/**/
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GiftRegistry.Models
{
    /**/
    /*
       Name
              ApplicationUser
           
       Purpose
              Holds all the important information for a user
           
       Author
              Sean Flaherty
           
       Date
              1/30/2018
     */
    /**/
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime BirthDate { get; internal set; }
        public string Name { get; internal set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    /**/
    /*
       Name
              ApplicatoinDbContext
           
       Purpose
              Allows us to access all the application user informaton from the database
           
       Author
              Sean Flaherty
           
       Date
              3/30/2018
     */
    /**/
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}