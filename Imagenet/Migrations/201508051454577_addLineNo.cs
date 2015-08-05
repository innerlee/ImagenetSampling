namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLineNo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "LineNo", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Images", "LineNo");
        }
    }
}
