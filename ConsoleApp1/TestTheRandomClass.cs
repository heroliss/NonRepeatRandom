using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomRandom;
namespace Test
{
    class TestTheRandomClass
    {
        static void Main(string[] args)
        {
            int count = 0;
            Console.WriteLine("start..");
            //foreach (int item in new NoRepeatRandom(-2000, 2000))
            //{
            //    count++;
            //}

            //Console.WriteLine("count:{0}", count);
            Console.WriteLine("-------------------");
            NoRepeatRandom r = new NoRepeatRandom(0, 1000);
            Console.WriteLine("{0}", r.CurrentMode);
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine(r.Next());
            }
            Console.WriteLine("{0}", r.CurrentMode);
            
            Console.WriteLine("end");
            Console.Read();
        }
    }
}
