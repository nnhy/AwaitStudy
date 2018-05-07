using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NewLife.Log;
using NewLife.Security;

namespace AwaitTest
{
    class Await
    {
        public static async void Start()
        {
            WriteStep("主函数1");
            // 异步执行，不等待
            Task.Run(Test);

            Thread.Sleep(100);
            WriteStep("主函数2");
            // 同步执行第一状态机，异步执行二三状态机，并等待返回
            await Test();

            WriteStep("主函数3");
            // 新开异步任务执行Test，等待新任务并Unwrap解包
            await Task.Run(Test);

            WriteStep("主函数4");
            // 新开异步任务执行Test，等待新任务并Unwrap解包
            Task.Run(Test).Wait();

            WriteStep("主函数5");
            // 新开异步任务执行Test，并等待新任务完成，不解包，仅执行第一状态机
            await Task.Factory.StartNew(Test);

            WriteStep("主函数6");
            // 新开异步任务执行Test，并等待新任务完成，不解包，仅执行第一状态机
            Task.Factory.StartNew(Test).Wait();

            WriteStep("主函数7");
        }

        static Int32 _gid;
        static async Task<Int32> Test()
        {
            var key = Interlocked.Increment(ref _gid);
            WriteStep("子函数1 " + key);

            await Task.Delay(1300);
            WriteStep("子函数2 " + key);

            await Task.Delay(1700);
            WriteStep("子函数3 " + key);

            return key;
        }

        static void WriteStep(String name)
        {
            var ctx = SynchronizationContext.Current;
            XTrace.WriteLine("线程{0} 任务{1} {2} {3}", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, name, ctx?.GetType().Name);
        }
    }
}