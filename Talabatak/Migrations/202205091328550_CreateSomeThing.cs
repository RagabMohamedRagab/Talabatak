namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateSomeThing : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Stores", "SubCategory_Id", "dbo.SubCategories");
            DropIndex("dbo.Stores", new[] { "SubCategory_Id" });
            DropColumn("dbo.Stores", "SubCategory_Id");
            DropTable("dbo.SubCategories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SubCategories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        Visiable = c.Boolean(nullable: false),
                        ImageUrl = c.String(),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        IsModified = c.Boolean(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedOn = c.DateTime(),
                        RestoredOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Stores", "SubCategory_Id", c => c.Long());
            CreateIndex("dbo.Stores", "SubCategory_Id");
            AddForeignKey("dbo.Stores", "SubCategory_Id", "dbo.SubCategories", "Id");
        }
    }
}
