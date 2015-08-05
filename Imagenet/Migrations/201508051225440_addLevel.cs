namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLevel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Synsets", "Level", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Synsets", "Level");
        }
    }
}
