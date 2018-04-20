namespace GiftRegistry.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeanFlaherty1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GiftLists", "GiftName", c => c.String(nullable: false, maxLength: 60));
            AddColumn("dbo.GiftLists", "UserId", c => c.String());
            AddColumn("dbo.GiftLists", "Bought", c => c.Boolean(nullable: false));
            DropColumn("dbo.GiftLists", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GiftLists", "Name", c => c.String(nullable: false, maxLength: 60));
            DropColumn("dbo.GiftLists", "Bought");
            DropColumn("dbo.GiftLists", "UserId");
            DropColumn("dbo.GiftLists", "GiftName");
        }
    }
}
