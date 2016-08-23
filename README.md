# AwaitStudy
async/await 在不同类型项目下的测试

#核心测试代码
```C#
public static async void Start()
{
    WriteStep("主函数1");

    Task.Run(Test);
    Thread.Sleep(100);
    WriteStep("主函数2");

    await Test();
    WriteStep("主函数3");
}

static async Task Test()
{
    var key = Rand.Next();
    WriteStep("子函数1 " + key);

    await Task.Delay(1300);
    WriteStep("子函数2 " + key);

    await Task.Delay(1700);
    WriteStep("子函数3 " + key);
}

static void WriteStep(String name)
{
    var ctx = SynchronizationContext.Current;
    XTrace.WriteLine("线程{0} 任务{1} {2} {3}", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, name, ctx?.GetType().Name);
}
```

#在不同平台下的测试结果
##控制台
```SQL
16:32:28.254  1 N - 线程1 任务 主函数1
16:32:28.263  5 Y 1 线程5 任务1 子函数1 1996556245
16:32:28.361  1 N - 线程1 任务 主函数2
16:32:28.362  1 N - 线程1 任务 子函数1 761185378
OK!
16:32:29.567  6 Y - 线程6 任务 子函数2 1996556245
16:32:29.676  5 Y - 线程5 任务 子函数2 761185378
16:32:31.269  6 Y - 线程6 任务 子函数3 1996556245
16:32:31.378  5 Y - 线程5 任务 子函数3 761185378
16:32:31.380  5 Y - 线程5 任务 主函数3
```
总结：<br>
1，Task.Run是一个独立世界，Test第一个await之前部分，在调用者线程执行。<br>
上面测试中，1号主线程建立了1号任务也就是5号线程。5号线程调用Test，所以子函数1在5号执行。<br>
2，await之后部分，可能用新线程执行，也可能继续用之前的调用者线程。<br>
上面测试，5号执行前面部分，6号执行后面部分。<br>
4，async/await实际会导致函数被编译器打算成为一个状态机，调用者线程执行第一个状态。<br>
上面Test会被打散成为await分割的三个部分：子函数1、子函数2、子函数3。<br>
其中，调用者线程会执行状态机第一部分，以及await函数。<br>
Task.Delay原理大概是向时间管理器注册一个到期时间，然后就返回。这一步由Test调用者线程执行。<br>
状态机第二部分，会由前一个await完成的线程执行，不一定跟调用者线程一致。<br>
所以，一个异步函数，N个await会导致函数被打散成为N+1个部分的状态，每一个状态执行后都建立异步任务，异步任务完成后，再从线程池抽取线程执行下一个状态！<br>

##WinForm
```SQL
16:40:01.526  1 N - 线程1 任务 主函数1 WindowsFormsSynchronizationContext
16:40:01.991 20 Y 1 线程20 任务1 子函数1 314447760 
16:40:02.089  1 N - 线程1 任务 主函数2 WindowsFormsSynchronizationContext
16:40:02.089  1 N - 线程1 任务 子函数1 1436843925 WindowsFormsSynchronizationContext
16:40:02.092  1 N - OK!
16:40:03.304 21 Y - 线程21 任务 子函数2 314447760 
16:40:03.398  1 N - 线程1 任务 子函数2 1436843925 WindowsFormsSynchronizationContext
16:40:05.005 21 Y - 线程21 任务 子函数3 314447760 
16:40:05.099  1 N - 线程1 任务 子函数3 1436843925 WindowsFormsSynchronizationContext
16:40:05.099  1 N - 线程1 任务 主函数3 WindowsFormsSynchronizationContext
```
总结：<br>
1，Task.Run会丢失当前上下文。上面的314447760没有上下文。<br>
2，如果调用者线程带有上下文，后续状态机执行，一律在该上下文中执行。<br>

##WPF
```SQL
线程10 任务 主函数1 DispatcherSynchronizationContext
线程6 任务553 子函数1 162945146 
线程10 任务 主函数2 DispatcherSynchronizationContext
线程10 任务 子函数1 406246526 DispatcherSynchronizationContext
线程6 任务 子函数2 162945146 
线程10 任务 子函数2 406246526 DispatcherSynchronizationContext
线程13 任务 子函数3 162945146 
线程10 任务 子函数3 406246526 DispatcherSynchronizationContext
线程10 任务 主函数3 DispatcherSynchronizationContext
```
总结：<br>
1，Task.Run建立了6号线程，状态机第一第二部分都在6号执行，第三部分在13号执行。
2，因为有上下文，await Test的三个部分都在该上下文执行。<br>

##WebMVC
```SQL
16:53:34.429  7 W - 线程7 任务 主函数1 AspNetSynchronizationContext
16:53:34.431 24 Y 6 线程24 任务6 子函数1 1354631946 
16:53:34.530  7 W - 线程7 任务 主函数2 AspNetSynchronizationContext
16:53:34.530  7 W - 线程7 任务 子函数1 1824671415 AspNetSynchronizationContext
16:53:35.741 21 Y - 线程21 任务 子函数2 1354631946 
16:53:37.443 21 Y - 线程21 任务 子函数3 1354631946 
16:53:37.646 21 W 13 线程21 任务13 子函数2 1824671415 AspNetSynchronizationContext
16:53:39.356 21 W 15 线程21 任务15 子函数3 1824671415 AspNetSynchronizationContext
16:53:39.356 21 W - 线程21 任务 主函数3 AspNetSynchronizationContext
```
总结：<br>
1，执行异常，异步模块或处理程序已完成，而当时仍有异步操作处于未定状态。<br>
2，7号Web线程执行Start，带有上下文。<br>
3，Task.Run开24号线程执行，没有上下文。状态机后面两部分在21号线程执行<br>
4，await Test的状态机后面两部分在21号执行，而不是7号，日志标识W说明上下文从7号转移到了21号。<br>
5，所以，即使带有上下文，执行状态机的线程不一定就是调用者线程。<br>