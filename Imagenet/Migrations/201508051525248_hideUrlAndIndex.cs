namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hideUrlAndIndex : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Images", "Index");
            DropColumn("dbo.Images", "Url");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Images", "Url", c => c.String());
            AddColumn("dbo.Images", "Index", c => c.Int());
        }
    }
}
