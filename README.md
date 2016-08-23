# AwaitStudy
async/await 在不同类型项目下的测试

#核心测试代码
```C#
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
```

#在不同平台下的测试结果
##控制台
```SQL
14:44:33.512  8 N - 线程8 任务 主函数1
14:44:33.540  8 N - 线程8 任务 主函数2
14:44:33.551 11 Y 1 线程11 任务1 子函数1
14:44:33.552  8 N - 线程8 任务 子函数1
OK!
14:44:34.561 12 Y - 线程12 任务 子函数2
14:44:34.561 11 Y - 线程11 任务 子函数2
14:44:34.564 12 Y - 线程12 任务 主函数3
```

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