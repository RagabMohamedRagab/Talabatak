namespace Talabatak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fineLateOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MetaDatas", "FineForLate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MetaDatas", "FineForLate");
        }
    }
}
