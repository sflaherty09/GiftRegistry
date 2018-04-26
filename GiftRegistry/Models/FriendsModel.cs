using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace GiftRegistry.Models
{
    public class FriendsModel
    {
        public int ID { get; set; }
        public string FriendID1 { get; set; }

        public string FriendID2 { get; set; }

        public bool Friend1NotificationFlag { get; set; }

        public bool Friend2NotificationFlag { get; set; }
    }
}