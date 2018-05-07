/**/
/*
    Name:

        UserModel
    
    Purpose: 
        
        To handle all information regarding a user into one place, making it easy
        to transfer a lot of data into view files
    
    Author:
        Sean Flaherty
 */
/**/
using System.Collections.Generic;

namespace GiftRegistry.Models
{
    /**/
    /*
       Name
              UserModel
           
       Purpose
              Shows user all the user's info in one place
           
       Author
              Sean Flaherty
           
       Date
              3/30/2018
     */
    /**/
    public class UserModel
    {
        public List<GiftList> Gifts { get; set; }
        public List<FriendsModel> Friends { get; set; }
        public List<ApplicationUser> AppUser { get; set; }
        
        public string UserID { get; set; }
    }
}