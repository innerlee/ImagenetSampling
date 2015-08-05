namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeSynIdTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ImgIds", "SynsetId", "dbo.SynIds");
            DropForeignKey("dbo.SynIds", "Wnid", "dbo.Synsets");
            DropIndex("dbo.SynIds", new[] { "Wnid" });
            DropIndex("dbo.ImgIds", new[] { "SynsetId" });
            AddColumn("dbo.Synsets", "SynsetId", c => c.Int());
            CreateIndex("dbo.ImgIds", "SynsetId");
            DropTable("dbo.SynIds");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SynIds",
                c => new
                    {
                        SynsetId = c.Int(nullable: false, identity: true),
                        Wnid = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.SynsetId);
            
            DropIndex("dbo.ImgIds", new[] { "SynsetId" });
            DropColumn("dbo.Synsets", "SynsetId");
            CreateIndex("dbo.ImgIds", "SynsetId");
            CreateIndex("dbo.SynIds", "Wnid");
            AddForeignKey("dbo.SynIds", "Wnid", "dbo.Synsets", "Wnid");
            AddForeignKey("dbo.ImgIds", "SynsetId", "dbo.SynIds", "SynsetId");
        }
    }
}
