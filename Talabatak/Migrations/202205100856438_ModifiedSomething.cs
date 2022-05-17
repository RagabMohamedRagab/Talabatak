namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedSomething : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Products", name: "CategoryId", newName: "SubCategoryId");
            RenameIndex(table: "dbo.Products", name: "IX_CategoryId", newName: "IX_SubCategoryId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Products", name: "IX_SubCategoryId", newName: "IX_CategoryId");
            RenameColumn(table: "dbo.Products", name: "SubCategoryId", newName: "CategoryId");
        }
    }
}
