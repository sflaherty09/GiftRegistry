namespace GiftRegistry.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeanFlaherty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftLists", "Link", c => c.String(nullable: false));
            DropColumn("dbo.GiftLists", "DateWanted");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GiftLists", "DateWanted", c => c.DateTime(nullable: false));
            DropColumn("dbo.GiftLists", "Link");
        }
    }
}
