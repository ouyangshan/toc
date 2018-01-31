using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test;
namespace toc
{
    class Program
    {
        static void Main(string[] args)
        {
            test1 toc = new test1();
            byte a = 0xff;
            byte b = 0x0f;
            a =(byte) (a & b);
            Console.WriteLine(Convert.ToString(c, 2).PadLeft(8, '0'));
            Console.ReadKey();
            
        }
    }
}
