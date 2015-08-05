namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImgIds",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        SynsetId = c.Int(),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.SynIds", t => t.SynsetId)
                .Index(t => t.SynsetId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImgIds", "SynsetId", "dbo.SynIds");
            DropIndex("dbo.ImgIds", new[] { "SynsetId" });
            DropTable("dbo.ImgIds");
        }
    }
}
