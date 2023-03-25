using HerstLib.IO;

namespace MagnumOpus.ECS
{
    public static class ThreadedWorker
    {
        private static readonly Thread[] _threads;
        private static readonly AutoResetEvent[] _blocks;
        private static readonly AutoResetEvent _allReady = new(false);
        private static volatile int _readyThreads;
        private static Action<int, int> Action;

        static ThreadedWorker()
        {
            FConsole.WriteLine("ThreadedWorker Started");
            Action = (i, j) => { };
            _threads = new Thread[Environment.ProcessorCount];
            _blocks = new AutoResetEvent[Environment.ProcessorCount];
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                _blocks[i] = new AutoResetEvent(false);
                _threads[i] = new Thread(ThreadLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _threads[i].Start(i);
            }
        }

        public static void Run(Action<int, int> action, int threads)
        {
            if (threads > Environment.ProcessorCount)
                threads = Environment.ProcessorCount;

            Action = action;
            _allReady.Reset();
            Interlocked.Exchange(ref _readyThreads, _readyThreads - threads);
            for (var i = 0; i < threads; i++)
                _blocks[i].Set();
            _allReady.WaitOne();
        }


        public static void ThreadLoop(object? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var idx = (int)id;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                if (_readyThreads >= _threads.Length)
                    _allReady.Set();

                _blocks[idx].WaitOne();
                Action.Invoke(idx, _threads.Length);
            }
        }
    }
}