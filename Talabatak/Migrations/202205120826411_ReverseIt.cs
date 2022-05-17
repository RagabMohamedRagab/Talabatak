namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReverseIt : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Stores", "Product_Id", "dbo.Products");
            DropIndex("dbo.Stores", new[] { "Product_Id" });
            AddColumn("dbo.Products", "StoreId", c => c.Long());
            CreateIndex("dbo.Products", "StoreId");
            AddForeignKey("dbo.Products", "StoreId", "dbo.Stores", "Id");
            DropColumn("dbo.Stores", "Product_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Stores", "Product_Id", c => c.Long());
            DropForeignKey("dbo.Products", "StoreId", "dbo.Stores");
            DropIndex("dbo.Products", new[] { "StoreId" });
            DropColumn("dbo.Products", "StoreId");
            CreateIndex("dbo.Stores", "Product_Id");
            AddForeignKey("dbo.Stores", "Product_Id", "dbo.Products", "Id");
        }
    }
}
