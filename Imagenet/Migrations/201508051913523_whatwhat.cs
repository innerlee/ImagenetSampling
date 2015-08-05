namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class whatwhat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Synsets", "TotalCount", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Synsets", "TotalCount");
        }
    }
}
