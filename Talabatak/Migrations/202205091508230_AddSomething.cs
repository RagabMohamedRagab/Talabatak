namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSomething : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductCategories", "Category_Id", c => c.Long());
            CreateIndex("dbo.ProductCategories", "Category_Id");
            AddForeignKey("dbo.ProductCategories", "Category_Id", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductCategories", "Category_Id", "dbo.Categories");
            DropIndex("dbo.ProductCategories", new[] { "Category_Id" });
            DropColumn("dbo.ProductCategories", "Category_Id");
        }
    }
}
