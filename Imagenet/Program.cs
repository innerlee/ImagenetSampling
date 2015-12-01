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
            // organize all the tasks.
            var m = new Manager();

            // TASK1: list all wnids
            // TASK2: filter wnids by counts
            // TASK3: output download links
            m.Top100();


            //m.Random100();

            //m.OutputAllLeafsWithEnouphImgs();


            //var path = "";
            //if (args.Count() > 0)
            //    path = args[0];

            //m.BatchResizeSubfolders(path);
            //m.GenerateMatlabTree();

            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
