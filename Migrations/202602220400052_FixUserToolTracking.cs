namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUserToolTracking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTools", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTools", "IsActive");
        }
    }
}
