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
总结：
1，Task.Run是一个独立世界，Test第一个await之前部分，在调用者线程执行。
上面测试中，1号主线程建立了1号任务也就是5号线程。5号线程调用Test，所以子函数1在5号执行。
2，await之后部分，可能用新线程执行，也可能继续用之前的调用者线程。
上面测试，5号执行前面部分，6号执行后面部分。
4，async/await实际会导致函数被编译器打算成为一个状态机，调用者线程执行第一个状态。
上面Test会被打散成为await分割的三个部分：子函数1、子函数2、子函数3。
其中，调用者线程会执行状态机第一部分，以及await函数。
Task.Delay原理大概是向时间管理器注册一个到期时间，然后就返回。这一步由Test调用者线程执行。
状态机第二部分，会由前一个await完成的线程执行，不一定跟调用者线程一致。
所以，一个异步函数，N个await会导致函数被打散成为N+1个部分的状态，每一个状态执行后都建立异步任务，异步任务完成后，再从线程池抽取线程执行下一个状态！

##WinForm
```SQL
14:45:55.056 10 N - 线程10 任务 主函数1 WindowsFormsSynchronizationContext
14:45:55.064 10 N - 线程10 任务 主函数2 WindowsFormsSynchronizationContext
14:45:55.064 10 N - 线程10 任务 子函数1 WindowsFormsSynchronizationContext
14:45:55.065  6 Y 1 线程6 任务1 子函数1 
14:45:56.066  6 Y - 线程6 任务 子函数2 
14:45:56.068 10 N - 线程10 任务 子函数2 WindowsFormsSynchronizationContext
14:45:56.068 10 N - 线程10 任务 主函数3 WindowsFormsSynchronizationContext
```

##WPF
```SQL
线程10 任务 主函数2 DispatcherSynchronizationContext
线程6 任务565 子函数1 
线程10 任务 子函数1 DispatcherSynchronizationContext
线程13 任务 子函数2 
线程10 任务 子函数2 DispatcherSynchronizationContext
线程10 任务 主函数3 DispatcherSynchronizationContext
```

##WebMVC
```SQL
14:47:32.956  6 W - 线程6 任务 主函数1 AspNetSynchronizationContext
14:47:32.956  6 W - 线程6 任务 主函数2 AspNetSynchronizationContext
14:47:32.957  6 W - 线程6 任务 子函数1 AspNetSynchronizationContext
14:47:32.957  7 Y 6 线程7 任务6 子函数1 
14:47:33.959  7 Y - 线程7 任务 子函数2 
14:47:36.581  7 W 12 线程7 任务12 子函数2 AspNetSynchronizationContext
14:47:36.581  7 W - 线程7 任务 主函数3 AspNetSynchronizationContext
```