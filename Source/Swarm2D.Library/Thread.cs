using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
#endif

namespace Swarm2D.Library
{
    public class Thread
    {
#if !WINDOWS_UWP
        System.Threading.Thread _thread;

        private ThreadStart _threadStart;

        public string Name
        {
            get { return _thread.Name; }
            set { _thread.Name = value; }
        }

        public Thread(ThreadStart threadStart)
        {
            _threadStart = threadStart;
            _thread = new System.Threading.Thread(ThreadMain);
        }

        private void ThreadMain()
        {
            _threadStart();
        }

        public void Start()
        {
            _thread.Start();
        }

        public static void Sleep(int miliSeconds)
        {
            System.Threading.Thread.Sleep(miliSeconds);
        }

#else
        private ThreadStart _threadStart;
        private Task _task;

        public string Name{ get; set; }

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
#endif
    }

    public delegate void ThreadStart();
}
