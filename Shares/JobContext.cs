using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace App.Scheduler
{
    /// <summary>任务上下文</summary>
    /// <remarks>
    /// JobContext.Currrent["db"] = xxx;
    /// </remarks>
    public class JobContext : Dictionary<string, object>
    {
        // 用线程 ID 来区分上下文
        public static object _lock = new object();
        private static Dictionary<int, JobContext> _contexts = new Dictionary<int, JobContext>();

        /// <summary>当前任务上下文</summary>
        public static JobContext Current
        {
            get
            {
                //var ctx = System.Runtime.Remoting.Messaging.CallContext.HostContext;
                var id = Thread.CurrentThread.ManagedThreadId;
                if (!_contexts.ContainsKey(id))
                {
                    lock (_lock)
                    {
                        _contexts.Add(id, new JobContext());
                        Trace.WriteLine("Build JobContext" + id);
                    }
                }
                return _contexts[id];
            }
        }

        /// <summary>删除当前任务上下文</summary>
        public static void RemoveCurrent()
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            lock (_lock)
            {
                _contexts.Remove(id);
                //Trace.WriteLine("Release JobContext" + id);
            }
        }
    }
}
