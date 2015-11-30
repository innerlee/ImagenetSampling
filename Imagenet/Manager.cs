using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using System.IO;
using System.Collections.Concurrent;
using System.Net;
using ImageProcessor.Imaging.Formats;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageResizer;

namespace Imagenet
{
    public class Manager
    {
        ImgnetContext db = new ImgnetContext();
        string path = @"D:\Backup\Source\Repos\Imagenet\Imagenet\Imagenet\data\";
        string imgpath = @"C:\imagenet_fall11_urls.txt";
        string lineno = @"D:\lineNo.txt";
        string linenoInt = @"D:\lineNoInt.txt";
        string output = @"D:\";
        string imgdir = @"D:\imgnetimg\";
        //List<string> lines;
        int capacity = 100000;

        int taken = 0;

        class OutputHelper
        {
            public int line;
            public string info;
        }

        public void BatchResizeSubfolders(string path)
        {
            if (path == "") path = @"D:\testimg\";
            var folders = Directory.GetDirectories(path);
            foreach (var f in folders)
            {
                Console.WriteLine($"== In folder {f}");
                BatchResizeImg(f);
            }
        }

        public void BatchResizeImg(string path)
        {
            if (path == "") path = @"D:\testimg\";
            var files = Directory.GetFiles(path).Select(f => f.ToLower()).Where(f => f.EndsWith(".jpeg")).ToList();
            foreach (var f in files)
            {
                ResizeImg(f);
                Console.Write(".");
                //Console.WriteLine($"file: {f}");
            }
            Console.WriteLine();
        }

        public void ResizeImg(string file)
        {
            var replace = true;

            if (file == "") file = @"D:\img.jpeg";
            var outfile = file;
            if (!replace)
                outfile = file.Replace(".jpeg", "_out" + DateTime.Now.ToString("MMddHHmmss") + ".jpeg");

            var minDimMax = 256;

            byte[] photoBytes = File.ReadAllBytes(file);

            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (var img = System.Drawing.Image.FromStream(inStream))
                {
                    var w = img.Width;
                    var h = img.Height;
                    //if (w > minDimMax && h > minDimMax)
                    //{
                    // Format is automatically detected though can be changed.
                    ISupportedImageFormat format = new JpegFormat { Quality = 90 };

                    Size size1 = new Size(minDimMax, minDimMax * 10);
                    Size size2 = new Size(minDimMax * 10, minDimMax);

                    var resize1 = new ResizeLayer(size1, ResizeMode.Max);
                    var resize2 = new ResizeLayer(size2, ResizeMode.Max);
                    ResizeLayer layer = w < h ? resize1 : resize2;
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            // Load, resize, set the format and quality and save an image.
                            imageFactory.Load(inStream)
                                        .Resize(layer)
                                        .Format(format)
                                        .Save(outfile);
                        }
                    }
                    //}
                    //else if (!replace)
                    //{
                    //    File.Copy(file, outfile);
                    //}
                }

            }
        }

        public void DownloadImages(string file)
        {
            if (file == "") file = @"";

            var salt = DateTime.Now.ToString("MMddHHmmss");
            var baseDir = imgdir + salt + "\\";
            Directory.CreateDirectory(baseDir);

            string[] lines = System.IO.File.ReadAllLines(file);
            var imgs = lines.Select(l => new ImgId { SynsetId = int.Parse(l) }).ToList();

            // Create queue of WebClient instances
            BlockingCollection<WebClient> ClientQueue = new BlockingCollection<WebClient>();
            // Initialize queue with some number of WebClient instances
            ClientQueue.Add(new WebClient());
            ClientQueue.Add(new WebClient());
            ClientQueue.Add(new WebClient());
            ClientQueue.Add(new WebClient());
            ClientQueue.Add(new WebClient());

            //// now process urls
            //foreach (var url in urls_to_download)
            //{
            //    var worker = ClientQueue.Take();
            //    worker.DownloadStringAsync(url, ...);
            //}

        }

        public void Top100()
        {
            var lines = new List<OutputHelper>();
            var imgids = new List<OutputHelper>();
            //var outSynList = new List<Synset>();
            //var infos = new List<string>();
            //var synsets = new List<Synset>();

            var list = db.Synsets.Where(s => s.LeafHeight == 0).OrderByDescending(s => s.ImgCount.Value).Take(10).ToList();
            foreach (var l in list)
            {
                var ids = db.ImgIds.Where(i => i.SynsetId == l.SynsetId).Take(12)
                    .Select(i => new OutputHelper { line = i.ImageId, info = l.Wnid + "\t" + l.Words }).ToList();
                imgids.AddRange(ids);
            }

            lines = imgids.OrderBy(i => i.line).ToList();

            WriteFiles(list, lines);

        }

        class Node
        {
            public int id { get; set; }
            public Node Parent { get; set; }
            public string Wnid { get; set; }
            public int Level { get; set; }
            public bool Used { get; set; }
            public string Word { get; set; }
        }

        public void GenerateMatlabTree()
        {
            var wnidpath = @"D:\imgnetimg\batch1\0806141731-synsetsBrief2.txt";
            var salt = DateTime.Now.ToString("MMddHHmmss-");
            var outFile = output + salt + "tree.txt";

            var dict = new Dictionary<string, Node>();

            string[] lines = File.ReadAllLines(wnidpath);
            var nodes = lines.Select(l =>
            {
                var split = l.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) return new Node { Wnid = "" };
                return new Node { Wnid = split[0], Used = true };
            }).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

            // load data from db.
            var data = db.Synsets.ToList();

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var n = nodes[i];
                var nn = data.Find(d => d.Wnid == n.Wnid);
                if (nn == null)
                {
                    Console.WriteLine($"wrong wnid {n.Wnid}, removed it.");
                    nodes.RemoveAt(i);
                    continue;
                }
                n.Level = nn.Level ?? 0;
                n.Word = nn.Words.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                dict.Add(n.Wnid, n);
            }

            var counter = 0;
            var cycle = 0;
            do
            {
                counter = 0;
                cycle++;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var n = nodes[i];
                    if (n.Parent == null)
                    {
                        var nn = data.Find(d => d.Wnid == n.Wnid);
                        if (nn == null)
                        {
                            Console.WriteLine($"wrong wnid {n.Wnid}, but keeping it.");
                            continue;
                        }
                        Console.WriteLine($"cycle {cycle}, id {i}, {nn.Wnid}");
                        if (nn.Parent == null)
                        {
                            Console.WriteLine($"cycle {cycle}, no parent for {n.Wnid}.");
                            continue;
                        }
                        if (dict.ContainsKey(nn.Parent.Wnid))
                        {
                            n.Parent = dict[nn.Parent.Wnid];
                        }
                        else
                        {
                            var pnode = new Node
                            {
                                Wnid = nn.Parent.Wnid,
                                Level = nn.Parent.Level ?? 0,
                                Used = false,
                                Word = nn.Parent.Words.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]
                            };
                            Console.WriteLine($"add {pnode.Wnid}");
                            nodes.Add(pnode);
                            n.Parent = pnode;
                            dict.Add(pnode.Wnid, pnode);
                        }
                        counter++;
                    }
                }
            } while (counter > 0);

            StreamWriter outTree = new StreamWriter(outFile, false);
            var treelist = nodes.OrderBy(n => n.Level).ToList();
            for (int i = 0; i < treelist.Count; i++)
            {
                var n = treelist[i];
                n.id = i + 1;
                outTree.WriteLine($"{i + 1}\t{(n.Parent == null ? 0 : n.Parent.id)}\t{n.Wnid}\t{n.Word}");
            }

            outTree.Flush();

        }

        public void OutputAllLeafsWithEnouphImgs()
        {
            var low = 800;
            var up = 1200;
            var r = new Random();

            var list = db.Synsets.Where(s => s.ImgCount > low && s.ImgCount < up && s.LeafHeight == 0).ToList();
            Console.WriteLine($"total {list.Count} categories.");

            var salt = DateTime.Now.ToString("MMddHHmmss-");
            var outUrlFile = output + salt + "leafs.txt";

            StreamWriter file = new StreamWriter(outUrlFile, false);
            foreach (var s in list)
            {
                file.WriteLine($"{s.Wnid}\t({s.ImgCount})\t{s.Words}\t({s.Glosses})");
            }
            file.Flush();

        }

        public void Random100()
        {
            var low = 800;
            var up = 1200;
            var take = 100;
            var size = 1200;
            var r = new Random();

            var tot = db.Synsets.Where(s => s.ImgCount > low && s.ImgCount < up && s.LeafHeight == 0).ToList();
            if (tot.Count > take)
            {
                for (int i = 0; i < take; i++)
                {
                    var temp = tot[i];
                    var j = r.Next(tot.Count);
                    tot[i] = tot[j];
                    tot[j] = temp;
                }
            }

            var list = tot.Take(take).ToList();

            var lines = new List<OutputHelper>();
            var imgids = new List<OutputHelper>();
            foreach (var l in list)
            {
                var ids = db.ImgIds.Where(i => i.SynsetId == l.SynsetId).Take(size)
                    .Select(i => new OutputHelper { line = i.ImageId, info = l.Wnid + "\t" + l.Words }).ToList();
                imgids.AddRange(ids);
            }
            lines = imgids.OrderBy(i => i.line).ToList();

            WriteFiles(list, lines);

        }

        void WriteFiles(List<Synset> list, List<OutputHelper> lines)
        {

            var salt = DateTime.Now.ToString("MMddHHmmss-");
            var outDownLinkFile = output + salt + "downlink.txt";
            var outImgUrlFile = output + salt + "imgurl.txt";
            var outSynsetsFile = output + salt + "synsets.txt";
            var outSynsetBriefFile = output + salt + "synsetBrief.txt";

            string line;
            var counter = 1;
            var j = 0;
            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(imgpath);
            System.IO.StreamWriter outDown = new System.IO.StreamWriter(outDownLinkFile, false);
            System.IO.StreamWriter outImg = new System.IO.StreamWriter(outImgUrlFile, false);
            System.IO.StreamWriter outSynsets = new System.IO.StreamWriter(outSynsetsFile, false);
            System.IO.StreamWriter outSynsetBrief = new System.IO.StreamWriter(outSynsetBriefFile, false);

            foreach (var item in list)
            {
                outDown.WriteLine($"http://www.image-net.org/download/synset?wnid={item.Wnid}&username=innerleees&accesskey=f9fe89003dc2e72798faafcc2444b01086415effes&release=latest&src=stanford");
                outSynsets.WriteLine(item.Wnid + "\t[" + item.Level.ToString() + "|" + item.LeafHeight.ToString() + "][" + item.ImgCount.ToString() + "] \t" + item.Words + "\t(" + item.Glosses + ")");
                outSynsetBrief.WriteLine($"{item.Wnid}\t{item.Words.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]}");
            }
            outDown.Flush();
            outSynsets.Flush();
            outSynsetBrief.Flush();

            while ((line = file.ReadLine()) != null && j < lines.Count)
            {
                if ((counter++) != lines[j].line) continue;
                var split = line.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) continue;
                outImg.WriteLine(split[0] + "\t" + lines[j].info + "\t" + split[1]);
                ++j;
            }
            outImg.Flush();

            file.Close();
        }

        public void InitLeafHeight()
        {
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                if (s.Children == null) s.LeafHeight = 0;
            }
            db.SaveChanges();
        }

        public void ComputeLeafHeight()
        {
            var modified = 0;
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                if (s.Children == null) continue;
                var num = s.Children.Max(c => c.LeafHeight) + 1;
                if (s.LeafHeight != num)
                {
                    s.LeafHeight = num;
                    modified++;
                }
            }
            Console.WriteLine("modifiled: " + modified.ToString());
            db.SaveChanges();
        }

        public void InitTotalCount()
        {
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                //s.TotalCount = s.ImgCount ?? 0;
                if (s.ImgCount == null) s.ImgCount = 0;
            }
            db.SaveChanges();
        }

        public void ComputeTotalCount()
        {
            var modified = 0;
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                if (s.Children == null) continue;
                var num = s.Children.Sum(c => c.ImgCount);
                if (s.TotalCount != num)
                {
                    s.TotalCount = num;
                    modified++;
                }
            }
            Console.WriteLine("modifiled: " + modified.ToString());
            db.SaveChanges();
        }

        public void CountImgs()
        {
            var pl = (from i in db.ImgIds
                          //orderby i.SynsetId
                      group i by i.SynsetId into grp
                      select new { SynsetId = grp.Key, count = grp.Count() }).OrderBy(a => a.SynsetId).ToList();

            var p2 = db.Synsets.OrderBy(s => s.SynsetId).ToList();

            var count = 0;
            var j = 0;
            for (int i = 0; i < pl.Count; i++)
            {
                while (p2[j].SynsetId != pl[i].SynsetId)
                {
                    j++;
                }
                p2[j].ImgCount = pl[i].count;
                count++;

                if (count % 10000 == 0) Console.WriteLine(count);
            }
            db.SaveChanges();

        }

        public void MoveDataToInt()
        {
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                s.SynsetId = int.Parse(s.Wnid.Substring(1));
            }

            db.SaveChanges();

        }


        public void BatchAddImageIntVersion()
        {
            var filename = linenoInt;

            string[] lines = System.IO.File.ReadAllLines(filename);
            var imgs = lines.Select(l => new ImgId { SynsetId = int.Parse(l) }).ToList();

            var count = imgs.Count / capacity;

            var i = 0;
            for (; i < count; i++)
            {
                db.BulkInsert(imgs.GetRange(i * capacity, capacity));
                Console.WriteLine(i * capacity);

            }
            db.BulkInsert(imgs.GetRange(i * capacity, imgs.Count % capacity));
        }

        //private void ProcessLinesOfImgs(List<string> lines)
        //{
        //    var imgs = lines.Select(l =>
        //    {
        //        var split = l.Split(new char[] { '\t', '_' }, 4, StringSplitOptions.RemoveEmptyEntries);
        //        if (split.Count() != 4) return new Image { Wnid = "" };
        //        return new Image { Wnid = split[1] };
        //        //return new Image { LineNo = int.Parse(split[0]), Wnid = split[1], Index = int.Parse(split[2]) };
        //    }).Where(s => s.Wnid != "").ToList();
        //    db.Images.AddRange(imgs);
        //    db.SaveChanges();
        //}

        //public void AddImgs()
        //{
        //    var filename = imgpath;
        //    int counter = 0;
        //    string line;

        //    //db.Images.RemoveRange(db.Images);
        //    //db.SaveChanges();
        //    //db.Database.ExecuteSqlCommand("truncate table [dbo.Images]");

        //    //var lines = new List<string>();
        //    //lines.Capacity = capacity;
        //    taken = 0;

        //    // Read the file and display it line by line.
        //    System.IO.StreamReader file = new System.IO.StreamReader(filename);
        //    while ((line = file.ReadLine()) != null)
        //    {
        //        counter++;
        //        taken++;

        //        var id = line.Substring(0, 9);
        //        if (id.Length < 9) continue;
        //        db.Images.Add(new Image { Wnid = id, LineNo = counter });

        //        //lines.Add(counter.ToString() + "\t" + line);

        //        if (taken == capacity)
        //        {
        //            //ProcessLinesOfImgs(lines);
        //            //lines.Clear();
        //            //taken = 0;
        //            db.SaveChanges();
        //            Console.WriteLine(counter);
        //        }

        //    }

        //    //ProcessLinesOfImgs(lines);

        //    file.Close();

        //    //string[] lines = System.IO.File.ReadAllLines(filename);
        //    //var imgs = lines.Select(l =>
        //    //{
        //    //    var split = l.Split(new char[] { '\t', '_' }, 2, StringSplitOptions.RemoveEmptyEntries);
        //    //    if (split.Count() != 3) return new Image { Wnid = "" };
        //    //    return new Image { Wnid = split[0], Index = int.Parse(split[1]), Url = split[2] };
        //    //}).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

        //    //db.Images.AddRange(imgs);

        //    //db.SaveChanges();

        //    Console.WriteLine("done.");
        //    Console.ReadLine();

        //}


        public void BatchAddImage()
        {
            var filename = lineno;

            string[] lines = System.IO.File.ReadAllLines(filename);
            var imgs = lines.Select(l => new Image { Wnid = l }).ToList();

            db.BulkInsert(imgs);
            //lines = null;
            //var count = imgs.Count / capacity;

            //var i = 0;
            //for (; i < count; i++)
            //{
            //    using (var context = new ImgnetContext())
            //    {
            //        context.Configuration.AutoDetectChangesEnabled = false;
            //        context.Configuration.ValidateOnSaveEnabled = false;
            //        context.Images.AddRange(imgs.GetRange(i * capacity, capacity));
            //        context.SaveChanges();
            //        Console.WriteLine(i * capacity);
            //    }
            //}
            //using (var context = new ImgnetContext())
            //{
            //    context.Images.AddRange(imgs.GetRange(i * capacity, imgs.Count % capacity));
            //    context.SaveChanges();
            //}


            //var synsets = db.Synsets.OrderBy(s => s.Wnid).ToList();

            //var L = glosses.Count;
            //if (synsets.Count != L)
            //{
            //    Console.WriteLine("lines count not match");
            //    return;
            //}

            //for (int i = 0; i < L; i++)
            //{
            //    if (synsets[i].Wnid == glosses[i].Wnid)
            //        synsets[i].Glosses = glosses[i].Glosses;
            //    else
            //        Console.WriteLine("not match line " + i.ToString());
            //}

            //db.SaveChanges();
            Console.WriteLine("done.");
            Console.ReadLine();

        }


        public void ProcessBigFile()
        {
            var filename = imgpath;
            int counter = 0;
            string line;

            //db.Images.RemoveRange(db.Images);
            //db.SaveChanges();
            //db.Database.ExecuteSqlCommand("truncate table [dbo.Images]");

            //var lines = new List<string>();
            //lines.Capacity = capacity;
            taken = 0;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            System.IO.StreamWriter file2 = new System.IO.StreamWriter(lineno, false);
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                taken++;

                var id = line.Substring(1, 8);
                if (id.Length < 8)
                {
                    Console.WriteLine("broken line: " + counter.ToString());
                    continue;
                }

                file2.WriteLine(id);
                //db.Images.Add(new Image { Wnid = id, LineNo = counter });

                //lines.Add(counter.ToString() + "\t" + line);

                if (taken == capacity)
                {
                    //ProcessLinesOfImgs(lines);
                    //lines.Clear();
                    taken = 0;
                    //db.SaveChanges();
                    Console.WriteLine(counter);
                }

            }
            file2.Flush();

            //ProcessLinesOfImgs(lines);

            file.Close();

            //string[] lines = System.IO.File.ReadAllLines(filename);
            //var imgs = lines.Select(l =>
            //{
            //    var split = l.Split(new char[] { '\t', '_' }, 2, StringSplitOptions.RemoveEmptyEntries);
            //    if (split.Count() != 3) return new Image { Wnid = "" };
            //    return new Image { Wnid = split[0], Index = int.Parse(split[1]), Url = split[2] };
            //}).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

            //db.Images.AddRange(imgs);

            //db.SaveChanges();

            Console.WriteLine("done.");
            Console.ReadLine();

        }

        public void LoadWords()
        {
            var filename = path + "words.txt";
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while ((line = file.ReadLine()) != null)
            {
                var split = line.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) continue;
                db.Synsets.AddOrUpdate(s => s.Wnid, new Synset { Wnid = split[0], Words = split[1] });
                if (counter % 100 == 0) db.SaveChanges();
                Console.WriteLine(counter++);
            }
            db.SaveChanges();

            file.Close();

            // Suspend the screen.
            Console.ReadLine();

        }

        public int ScanParent()
        {
            var c = 0;

            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                if (s.Parent != null && s.Level != s.Parent.Level + 1)
                {
                    s.Level = s.Parent.Level + 1;
                    c++;
                }
            }
            db.SaveChanges();

            Console.WriteLine(c.ToString() + " lines changed.");
            Console.WriteLine("done.");
            Console.ReadLine();
            return c;
        }

        public void ResetLevel()
        {
            var list = db.Synsets.ToList();
            foreach (var s in list)
            {
                s.Level = 0;
            }
            db.SaveChanges();

            Console.WriteLine("done.");
            Console.ReadLine();
        }

        public void ClearAndLoadWords()
        {
            db.Synsets.RemoveRange(db.Synsets);
            db.SaveChanges();

            var filename = path + "words.txt";

            string[] lines = System.IO.File.ReadAllLines(filename);
            var synsets = lines.Select(l =>
            {
                var split = l.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) return new Synset { Wnid = "" };
                return new Synset { Wnid = split[0], Words = split[1] };
            }).Where(s => s.Wnid != "").ToList();

            db.Synsets.AddRange(synsets);
            db.SaveChanges();
        }


        public void AddIsAvailable()
        {
            var filename = path + "imagenet.synset.txt";

            string[] lines = System.IO.File.ReadAllLines(filename);
            var isa = lines.Where(l => l != null && l.Trim() != "").OrderBy(l => l).ToList();

            var list = db.Synsets.OrderBy(s => s.Wnid).ToList();

            var c = 0;
            for (int i = 0; i < isa.Count; i++)
            {
                while (list[c].Wnid != isa[i]) ++c;

                list[c].IsAvailable = true;
            }

            db.SaveChanges();

            Console.WriteLine("done.");
            Console.ReadLine();

        }

        public void AddParents()
        {
            var filename = path + "wordnet.is_a.txt";

            string[] lines = System.IO.File.ReadAllLines(filename);
            var isa = lines.Select(l =>
            {
                var split = l.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) return new Synset { Wnid = "" };
                return new Synset { Wnid = split[1], ParentId = split[0] };
            }).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

            var list = db.Synsets.OrderBy(s => s.Wnid).ToList();

            var c = 0;
            for (int i = 0; i < isa.Count; i++)
            {
                while (list[c].Wnid != isa[i].Wnid) ++c;

                list[c].ParentId = isa[i].ParentId;
            }

            db.SaveChanges();

            Console.WriteLine("done.");
            Console.ReadLine();

        }

        public void CheckParentIsSingle()
        {
            var filename = path + "wordnet.is_a.txt";

            string[] lines = System.IO.File.ReadAllLines(filename);
            var isa = lines.Select(l =>
            {
                var split = l.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) return new Synset { Wnid = "" };
                return new Synset { Wnid = split[1], ParentId = split[0] };
            }).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

            var list = isa.Select(i => i.Wnid).ToList().Distinct();

            if (list.Count() != isa.Count) Console.WriteLine("not equal.");
            else Console.WriteLine("equal.");

            //db.SaveChanges();

            Console.WriteLine("done.");
            Console.ReadLine();

        }

        public void BatchLoadGlosses()
        {
            var filename = path + "gloss.txt";

            string[] lines = System.IO.File.ReadAllLines(filename);
            var glosses = lines.Select(l =>
            {
                var split = l.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() != 2) return new Synset { Wnid = "" };
                return new Synset { Wnid = split[0], Glosses = split[1] };
            }).Where(s => s.Wnid != "").OrderBy(s => s.Wnid).ToList();

            var synsets = db.Synsets.OrderBy(s => s.Wnid).ToList();

            var L = glosses.Count;
            if (synsets.Count != L)
            {
                Console.WriteLine("lines count not match");
                return;
            }

            for (int i = 0; i < L; i++)
            {
                if (synsets[i].Wnid == glosses[i].Wnid)
                    synsets[i].Glosses = glosses[i].Glosses;
                else
                    Console.WriteLine("not match line " + i.ToString());
            }

            db.SaveChanges();
            Console.ReadLine();

        }

        public void BatchLoadWords()
        {
            var filename = path + "words.txt";
            int counter = 0;

            string[] lines = System.IO.File.ReadAllLines(filename);
            var synsets = lines.Select(l =>
              {
                  var split = l.Split(new char[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                  if (split.Count() != 2) return new Synset { Wnid = "" };
                  return new Synset { Wnid = split[0], Words = split[1] };
              }).Where(s => s.Wnid != "").ToList();

            foreach (var s in synsets)
            {
                db.Synsets.AddOrUpdate(s);
                Console.WriteLine(counter++);
            }

            Console.ReadLine();

        }
    }
}
