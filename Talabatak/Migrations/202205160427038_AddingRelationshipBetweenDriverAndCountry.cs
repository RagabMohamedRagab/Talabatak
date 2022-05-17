namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRelationshipBetweenDriverAndCountry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Drivers", "CountryId", c => c.Long());
            CreateIndex("dbo.Drivers", "CountryId");
            AddForeignKey("dbo.Drivers", "CountryId", "dbo.Countries", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Drivers", "CountryId", "dbo.Countries");
            DropIndex("dbo.Drivers", new[] { "CountryId" });
            DropColumn("dbo.Drivers", "CountryId");
        }
    }
}
