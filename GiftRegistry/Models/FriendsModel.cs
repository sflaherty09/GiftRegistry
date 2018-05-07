/**/
/*
    Name:

        FriendsModel
    
    Purpose: 
        
        To handle all information regarding a user's friends. Holds all friend data
        as well as notification flags 
    
    Author:
        Sean Flaherty
 */
/**/

namespace GiftRegistry.Models
{
    /**/
    /*
       Name
              FriendsModel
           
       Purpose
              Holds all the application's Friend information
              including both friend ids and notificatoin flags
           
       Author
              Sean Flaherty
           
       Date
              3/30/2018
     */
    /**/
    public class FriendsModel
    {
        public int ID { get; set; }
        public string FriendID1 { get; set; }

        public string FriendID2 { get; set; }

        public bool Friend1NotificationFlag { get; set; }

        public bool Friend2NotificationFlag { get; set; }
    }
}