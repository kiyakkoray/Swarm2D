using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swarm2D.Engine.Core;
using Swarm2D.Library;

namespace Swarm2D.UniversalWindowsPlatform.Framework
{
    public class Thread : IThread
    {
        private ThreadStart _threadStart;
        private Task _task;

        public string Name { get; set; }

        public Thread(ThreadStart threadStart)
        {
            _threadStart = threadStart;
        }

        public void Start()
        {
            _task = new Task(ThreadMain, TaskCreationOptions.LongRunning);
            _task.Start();
        }

        private void ThreadMain()
        {
            _threadStart();
        }

        public static void Sleep(int miliSeconds)
        {
            Task.Delay(miliSeconds).Wait();
        }
    }

}
