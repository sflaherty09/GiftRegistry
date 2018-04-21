using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Models
{
    public class FriendsModel
    {
        private List<string> friends = new List<string>();

        public void AddFriend(string friendID)
        {
            // sent here after user accepts request
            friends.Add(friendID);
        }

        public void SendRequest()
        {
            // SendGrid email asking someone to be friend
            // Include a link that they will send them to a page with two button options
            // Also the page will include their basic account info
        }

        public List<string> GetFriends()
        {
            return friends;
        }
    }
}