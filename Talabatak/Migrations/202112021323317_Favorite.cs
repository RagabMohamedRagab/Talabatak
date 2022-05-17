namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Favorite : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserFavoriteProducts",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProductId = c.Long(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.ProductId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserFavoriteProducts", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserFavoriteProducts", "ProductId", "dbo.Products");
            DropIndex("dbo.UserFavoriteProducts", new[] { "UserId" });
            DropIndex("dbo.UserFavoriteProducts", new[] { "ProductId" });
            DropTable("dbo.UserFavoriteProducts");
        }
    }
}
