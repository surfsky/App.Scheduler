using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using App.Components;

namespace App.Schedule
{
    /// <summary>
    /// 连接测试任务
    /// </summary>
    public class ConnectJob : IJobRunner
    {
        public bool Run(DateTime dt, string data)
        {
            try
            {
                HttpHelper.Get(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>空任务，休眠1秒</summary>
    public class DummyJob : IJobRunner
    {
        public bool Run(DateTime dt, string data)
        {
            int seconds = data.IsNullOrEmpty() ? 1 : int.Parse(data);
            Thread.Sleep(seconds*1000);
            return true;
        }
    }

    /// <summary>随机成功任务。可用于模拟测试任务依赖逻辑。</summary>
    public class RandomJob : IJobRunner
    {
        Random _random = new Random();
        public bool Run(DateTime dt, string data)
        {
            return _random.Next(0, 100) >= 50;
        }
    }

    /// <summary>Exe运行任务</summary>
    public class ExeJob : IJobRunner
    {
        public bool Run(DateTime dt, string data)
        {
            var process = new Process();
            process.StartInfo.FileName = data;
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => {};
            process.Start();
            process.WaitForExit();
            return process.ExitCode >= 0;
        }
    }

    /// <summary>Perl脚本任务</summary>
    public class PerlJob : IJobRunner
    {
        public bool Run(DateTime dt, string data)
        {
            var process = new Process();
            process.StartInfo.FileName = "Perl.exe";
            process.StartInfo.Arguments = data;
            process.Start();
            process.WaitForExit();
            return process.ExitCode >= 0;
        }
    }

    /// <summary>Python脚本任务</summary>
    public class PythonJob : IJobRunner
    {
        public bool Run(DateTime dt, string data)
        {
            var process = new Process();
            process.StartInfo.FileName = "Python.exe";
            process.StartInfo.Arguments = data;
            process.Start();
            process.WaitForExit();
            return process.ExitCode >= 0;
        }
    }

}
