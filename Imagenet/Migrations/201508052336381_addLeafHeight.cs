namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLeafHeight : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Synsets", "LeafHeight", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Synsets", "LeafHeight");
        }
    }
}
