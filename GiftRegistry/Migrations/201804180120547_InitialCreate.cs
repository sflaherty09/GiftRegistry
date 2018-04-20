namespace GiftRegistry.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GiftLists",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 60),
                        //DateWanted = c.DateTime(nullable: false),
                        Rating = c.Int(nullable: false),
                        Category = c.String(nullable: false, maxLength: 30),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Link = c.String(nullable: false)
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GiftLists");
        }
    }
}
