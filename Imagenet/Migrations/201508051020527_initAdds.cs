namespace Imagenet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initAdds : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        ImageId = c.Int(nullable: false, identity: true),
                        Wnid = c.String(maxLength: 16),
                        Index = c.Int(),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.ImageId)
                .ForeignKey("dbo.Synsets", t => t.Wnid)
                .Index(t => t.Wnid);
            
            CreateTable(
                "dbo.Synsets",
                c => new
                    {
                        Wnid = c.String(nullable: false, maxLength: 16),
                        Words = c.String(),
                        Glosses = c.String(),
                        IsAvailable = c.Boolean(),
                        ParentId = c.String(maxLength: 16),
                    })
                .PrimaryKey(t => t.Wnid)
                .ForeignKey("dbo.Synsets", t => t.ParentId)
                .Index(t => t.ParentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Synsets", "ParentId", "dbo.Synsets");
            DropForeignKey("dbo.Images", "Wnid", "dbo.Synsets");
            DropIndex("dbo.Synsets", new[] { "ParentId" });
            DropIndex("dbo.Images", new[] { "Wnid" });
            DropTable("dbo.Synsets");
            DropTable("dbo.Images");
        }
    }
}
