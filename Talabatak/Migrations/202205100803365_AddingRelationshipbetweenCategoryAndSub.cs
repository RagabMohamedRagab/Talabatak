namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRelationshipbetweenCategoryAndSub : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ProductCategories", name: "Category_Id", newName: "CategoryId");
            RenameIndex(table: "dbo.ProductCategories", name: "IX_Category_Id", newName: "IX_CategoryId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ProductCategories", name: "IX_CategoryId", newName: "IX_Category_Id");
            RenameColumn(table: "dbo.ProductCategories", name: "CategoryId", newName: "Category_Id");
        }
    }
}
