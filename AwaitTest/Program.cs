using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewLife.Log;

namespace AwaitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            Await.Start();

            Console.WriteLine("OK!");
            Console.ReadKey(true);
        }

    }
}