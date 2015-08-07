using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imagenet
{
    class Program
    {
        static void Main(string[] args)
        {
            var m = new Manager();
            //m.Random100();

            m.OutputAllLeafsWithEnouphImgs();


            //var path = "";
            //if (args.Count() > 0)
            //    path = args[0];

            //m.BatchResizeSubfolders(path);

            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
