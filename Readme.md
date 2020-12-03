# App.Scheduler 任务调度引擎


## 1.功能


- 调度表达式参照  cron
    (1) 顺序调整为：年 月 日 时 分 周
    (2) 每个部分可用逗号分隔
    (3) 只保留 * 通配符
- 含依赖逻辑
- 含成功后重试逻辑（下次运行间隔）
- 含失败后重试逻辑（重试间隔、重试次数限制）
- 可以用配置文件，也可以完全用代码创建任务配置对象来运行任务。
- 任务包含 Data 属性，作为任务调用参数。如测试网站可访问性任务的URL，运行脚本任务的脚本路径等。
- 任务采用线程启动
- 支持任务上下文变量 JobContext，任务启动时创建，任务释放时自动释放。

## 2.项目

- App.Scheduler    调度引擎。输出 App.Scheduler.dll
- App.Consoler     内置调度引擎的控制台程序。输出 App.Consoler.exe


## 3.Nuget 安装
```   
install-package App.Scheduler        // netframework 4.5 版本
install-package App.SchedulerStd     // netstandard 2.0 版本
```

## 4.使用

### 拷贝文件
```
App.Scheduler.dll
Scheduler.config
Log4Net.dll
Newstonsoft.Json.dll
```

### 实现任务逻辑

（1）引用App.Scheduler.dll
（2）实现接口 IJobRunner（或继承Job)，实现任务处理逻辑
```
public class MyJob : IJobRunner
{
    public bool Run(DateTime dt, string data)
    {
       return true;
    }
}
```            

### 配置

用代码创建 ScheduleConfig 对象
或修改 Scheduler.config json 文件（详见后）

### 运行

运行App.Consoler.exe（或实现自己的宿主程序）
![](./Snap/App.Consoler.png?raw=true)


## 5.内置的任务运行器

    DummyJob       : 空任务，停X秒后返回true
    RandomJob      : 随机任务，随机返回true、false，可用于测试任务依赖。
    ConnectJob     : 连接任务，可测试某个网站的可连接性
    ApplicationJob : 运行exe程序，若返回值大等于0，则返回true，
    PerlJob        : 运行perl脚本，若返回值大等于0，则返回true，
    PythonJob      : 运行python脚本，若返回值大等于0，则返回true，

## 6. Scheduler.config 示例
```
{
  "Sleep": 200,                                           // 任务引擎每次循环休息的毫秒数
  "LogDt": "2017-11-28 19:12:41",                         // 最后记录的时间
  "Jobs": [                                               // 任务列表
    {                                                     // 
      "Name": "任务",                                     // 任务名称
      "Enable": true,                                     // 是否有效
      "Schedule": "* * * 1,2 0 *",                        // 调度表达式，格式为：年月日时分周，此表达式为每天的1、2点运行
      "Status": "Success",                                // 最后运行状态
      "LastRunDt": "2017-11-28 19:12:17",                 // 最后运行时间
      "Success": "0000-00-00 00:00:10 0/9",               // 成功后间隔10秒钟再次运行
      "Failure": "0000-00-00 00:00:02 3/9",               // 失败后间隔2秒钟再次运行，已失败3次，最多失败9次
      "Runner": "App.Scheduler.RandomJob, App.Scheduler",   // IJobRunner 类型名，运行器的逻辑实现
      "Data": "http://www.baidu.com",                     // 附加参数，供Runner运行时作为参数传入
      "Dependency": [
        {
          "Name": "子任务1",
          "Enable": true,
          "Schedule": "* * * * * *",
          "Status": "Success",
          "LastRunDt": "2017-11-28 19:12:17",
          "Success": "0000-00-00 00:00:10 0/9",
          "Failure": "0000-00-00 00:00:02 0/9",
          "Runner": "App.Scheduler.RandomJob, App.Scheduler",
          "Dependency": []
        },
        {
          "Name": "子任务2",
          "Enable": true,
          "Schedule": "* * * * * *",
          "Status": "Success",
          "LastRunDt": "2017-11-28 19:12:17",
          "Success": "0000-00-00 00:00:10 0/9",
          "Failure": "0000-00-00 00:00:02 0/9",
          "Runner": "App.Scheduler.RandomJob, App.Scheduler",
          "Dependency": []
        }
      ]
    },
    {
      "Name": "Dummy",
      "Enable": false,
      "Schedule": "* * * * * *",
      "Status": "Success",
      "LastRunDt": "2017-11-28 19:12:37",
      "Success": "0000-00-00 00:01:00 0/9",
      "Failure": "0000-00-00 00:00:20 0/9",
      "Runner": "App.Scheduler.DummyJob, App.Scheduler",
      "Dependency": []
    }
  ]
}
```
## 7. 实现自己的宿主程序

参考 App.Consoler 项目，其核心代码如下：
```
engine = CreateEngine();
engine.ConfigFailure += (info) => { Logger.Error("{0}", info); Console.ReadKey(); };
engine.JobRunning += (job, info, _) =>
{
    JobContext.Current["name"] = "hello";
    Logger.Info("{0} {1}", job.Name, job.Data);
};
engine.JobFinish += (job, info, success) =>
{
    if (success)
        Logger.Info(@"{0} {1} √", job.Name, job.Data);
    else
        Logger.Warn(@"{0} {1} ×, times={2}, info={3}", job.Name, job.Data, job.Failure.TryTimes, info);
};

/// <summary>用代码构建引擎对象</summary>
static ScheduleEngine CreateEngine()
{
    ScheduleConfig cfg = new ScheduleConfig();
    cfg.LogDt = DateTime.Now;
    cfg.Sleep = 200;
    cfg.Jobs = new List<Job>();
    cfg.Jobs.Add(new Job()
    {
            Data = "http://www.baidu.com",
            Runner = typeof(ConnectJob),
            Name = "Connect",
            Success = new DateSpan(0, 0, 0, 0, 0, 30),
            Failure = new DateSpan(0, 0, 0, 0, 0, 0, 0, 9),
            Schedule = new Schedule("* * * * * *")
    });
    return new ScheduleEngine(cfg);
}

/// <summary>从配置中构建引擎对象</summary>
private static ScheduleEngine CreateEngineFromConfig()
{
    var folder = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
    string configFile = string.Format("{0}\\scheduler.config", folder);
    return new ScheduleEngine(configFile);
}
```

## 8. FAQ

- Q: 为什么开发该项目？
- A: Quartz 项目过于庞大，我并不需要; 讨厌年月颠倒的 cron 表达式; 练练手;


## 9.历史

- 2017-11-28  Init
- 2017-12-10  增加ApplicationJob, PerlJob, PythonJob
- 2017-12-11  解除对App.Components的依赖，避免依赖问题
- 2017-12-12  Nuget 部署: install-package App.Scheduler 
- 2018-11-07  项目名称更名为 App.Scheduler, 所有Task字样更名为Job（请注意修改Schedule.config文件), 增加 ScheduleEngine.Version 属性
- 2018-11-08  增加手动创建配置的示例
- 2020-12-03  采用线程运行任务；增加 JobContext 对象，用于在 Job 运行时中使用




## 11.参考

- CRON表达式： https://yq.aliyun.com/articles/62723#_Toc465868115
- 用Nuget部署程序包：https://www.cnblogs.com/surfsky/p/8072993.html


