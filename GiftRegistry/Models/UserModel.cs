using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GiftRegistry.Models
{
    public class UserModel
    {
        public List<GiftList> Gifts { get; set; }
        public List<FriendsModel> Friends { get; set; }
        public List<ApplicationUser> AppUser { get; set; }
    }
}