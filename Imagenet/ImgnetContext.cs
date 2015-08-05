namespace Imagenet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;

    public class ImgnetContext : DbContext
    {
        // Your context has been configured to use a 'ImgnetContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Imagenet.Model.ImgnetContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'ImgnetContext' 
        // connection string in the application configuration file.
        public ImgnetContext()
            : base("name=ImgnetContext")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Synset> Synsets { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        //public virtual DbSet<SynId> SynIds { get; set; }
        public virtual DbSet<ImgId> ImgIds { get; set; }
    }

    public class Synset
    {
        [Key, MaxLength(16)]
        public string Wnid { get; set; }
        public string Words { get; set; }
        public string Glosses { get; set; }
        public bool? IsAvailable { get; set; }
        public int? Level { get; set; }
        public int? ImgCount { get; set; }
        public int? TotalCount { get; set; }
        public int? SynsetId { get; set; }
        public int? LeafHeight { get; set; }

        public string ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Synset Parent { get; set; }
        [InverseProperty("Parent")]
        public List<Synset> Children { get; set; }
        [InverseProperty("Synset")]
        public List<Image> Images { get; set; }
        //[InverseProperty("Synset")]
        //public List<SynId> SynIds { get; set; }
    }

    //public class SynId
    //{
    //    [Key]
    //    public int SynsetId { get; set; }
    //    public string Wnid { get; set; }

    //    [ForeignKey("Wnid")]
    //    public virtual Synset Synset { get; set; }

    //    [InverseProperty("SynId")]
    //    public List<ImgId> ImgIds { get; set; }
    //}

    public class ImgId
    {
        [Key]
        public int ImageId { get; set; }
        [Index]
        public int? SynsetId { get; set; }
    }

    public class Image
    {
        public int ImageId { get; set; }
        [Index, MaxLength(16)]
        public string Wnid { get; set; }
        //public int? Index { get; set; }
        public int? LineNo { get; set; }
        //public string Url { get; set; }

        [ForeignKey("Wnid")]
        public virtual Synset Synset { get; set; }
    }

}