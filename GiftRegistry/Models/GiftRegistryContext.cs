using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GiftRegistry.Models
{
    /**/
    /*
       Name
              GiftRegistryContext
           
       Purpose
              Allows us to access all the gift list informaton from the database
           
       Author
              Sean Flaherty
           
       Date
              1/30/2018
     */
    /**/
    public class GiftRegistryContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public GiftRegistryContext() : base("name=GiftRegistryContext")
        {
        }

        public System.Data.Entity.DbSet<GiftRegistry.Models.GiftList> GiftLists { get; set; }
    }
}
