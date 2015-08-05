namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class switchToInt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SynIds",
                c => new
                    {
                        SynsetId = c.Int(nullable: false, identity: true),
                        Wnid = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.SynsetId)
                .ForeignKey("dbo.Synsets", t => t.Wnid)
                .Index(t => t.Wnid);
            
            AddColumn("dbo.Synsets", "ImgCount", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SynIds", "Wnid", "dbo.Synsets");
            DropIndex("dbo.SynIds", new[] { "Wnid" });
            DropColumn("dbo.Synsets", "ImgCount");
            DropTable("dbo.SynIds");
        }
    }
}
