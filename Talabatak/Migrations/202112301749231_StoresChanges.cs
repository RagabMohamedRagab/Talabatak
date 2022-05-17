namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoresChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "OfficialEmail", c => c.String());
            AddColumn("dbo.Stores", "TaxReportImageUrl", c => c.String());
            AddColumn("dbo.Stores", "TaxImageUrl", c => c.String());
            AddColumn("dbo.Stores", "ValueAddedTaxImageUrl", c => c.String());
            AddColumn("dbo.Stores", "TaxReportNumber", c => c.String());
            AddColumn("dbo.Stores", "ValueAddedTax", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stores", "ValueAddedTax");
            DropColumn("dbo.Stores", "TaxReportNumber");
            DropColumn("dbo.Stores", "ValueAddedTaxImageUrl");
            DropColumn("dbo.Stores", "TaxImageUrl");
            DropColumn("dbo.Stores", "TaxReportImageUrl");
            DropColumn("dbo.Stores", "OfficialEmail");
        }
    }
}
