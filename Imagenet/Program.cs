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
            m.Random100();
            //m.DownloadImages("");

            Console.WriteLine("done.");
            Console.ReadLine();
        }
    }
}
