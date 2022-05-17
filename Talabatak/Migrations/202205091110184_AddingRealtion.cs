namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRealtion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductCategories", "ProductCategory_Id", c => c.Long());
            CreateIndex("dbo.ProductCategories", "ProductCategory_Id");
            AddForeignKey("dbo.ProductCategories", "ProductCategory_Id", "dbo.ProductCategories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductCategories", "ProductCategory_Id", "dbo.ProductCategories");
            DropIndex("dbo.ProductCategories", new[] { "ProductCategory_Id" });
            DropColumn("dbo.ProductCategories", "ProductCategory_Id");
        }
    }
}
