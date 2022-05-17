namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRelationshipBetweenDriverAndCityi : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StoreOrderItems", "ProductId", "dbo.Products");
            DropIndex("dbo.StoreOrderItems", new[] { "ProductId" });
            AlterColumn("dbo.StoreOrderItems", "ProductId", c => c.Long());
            CreateIndex("dbo.StoreOrderItems", "ProductId");
            AddForeignKey("dbo.StoreOrderItems", "ProductId", "dbo.Products", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreOrderItems", "ProductId", "dbo.Products");
            DropIndex("dbo.StoreOrderItems", new[] { "ProductId" });
            AlterColumn("dbo.StoreOrderItems", "ProductId", c => c.Long(nullable: false));
            CreateIndex("dbo.StoreOrderItems", "ProductId");
            AddForeignKey("dbo.StoreOrderItems", "ProductId", "dbo.Products", "Id", cascadeDelete: true);
        }
    }
}
