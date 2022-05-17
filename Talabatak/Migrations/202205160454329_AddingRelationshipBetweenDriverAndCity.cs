namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingRelationshipBetweenDriverAndCity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Drivers", "CountryId", "dbo.Countries");
            DropIndex("dbo.Drivers", new[] { "CountryId" });
            AddColumn("dbo.Drivers", "CityId", c => c.Long());
            CreateIndex("dbo.Drivers", "CityId");
            AddForeignKey("dbo.Drivers", "CityId", "dbo.Cities", "Id");
            DropColumn("dbo.Drivers", "CountryId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Drivers", "CountryId", c => c.Long());
            DropForeignKey("dbo.Drivers", "CityId", "dbo.Cities");
            DropIndex("dbo.Drivers", new[] { "CityId" });
            DropColumn("dbo.Drivers", "CityId");
            CreateIndex("dbo.Drivers", "CountryId");
            AddForeignKey("dbo.Drivers", "CountryId", "dbo.Countries", "Id");
        }
    }
}
