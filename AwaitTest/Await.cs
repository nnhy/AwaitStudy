using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewLife.Log;

namespace AwaitTest
{
    class Await
    {
        public static async void Start()
        {
            WriteStep("主函数1");

            Task.Run(Test);

            WriteStep("主函数2");

            await Test();

            WriteStep("主函数3");
        }

        static async Task Test()
        {
            WriteStep("子函数1");

            await Task.Delay(1000);

            WriteStep("子函数2");
        }

        static void WriteStep(String name)
        {
            var ctx = SynchronizationContext.Current;
            XTrace.WriteLine("线程{0} 任务{1} {2} {3}", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, name, ctx?.GetType().Name);
        }
    }
}