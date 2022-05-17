namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRelationBetweenStoreProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "Product_Id", c => c.Long());
            CreateIndex("dbo.Stores", "Product_Id");
            AddForeignKey("dbo.Stores", "Product_Id", "dbo.Products", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Stores", "Product_Id", "dbo.Products");
            DropIndex("dbo.Stores", new[] { "Product_Id" });
            DropColumn("dbo.Stores", "Product_Id");
        }
    }
}
