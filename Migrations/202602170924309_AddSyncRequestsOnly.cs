namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSyncRequestsOnly : DbMigration
    {
        public override void Up()
        {
            CreateTable(
    "dbo.SyncRequests",
    c => new
    {
        Id = c.Int(nullable: false, identity: true),
        EmployeeId = c.Int(nullable: false),
        Email = c.String(),
        CompanyId = c.Int(nullable: false),
        ToolId = c.Int(nullable: false),
        Status = c.String(),
        CreatedAt = c.DateTime(nullable: false),
    })
    .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.SyncRequests");
        }
    }
}
