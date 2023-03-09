namespace MagnumOpus.ECS
{
    public static class ThreadedWorker
    {
        private static readonly Thread[] _threads;
        private static readonly AutoResetEvent[] _blocks;
        private static readonly AutoResetEvent _allReady = new(false);
        private static volatile int _readyThreads;
        private static Action<int> Action;

        static ThreadedWorker()
        {
            Action = Thread.Sleep;
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

        public static void Run(Action<int> action, int threads)
        {
            Action = action;
            _ = _allReady.Reset();
            _ = Interlocked.Exchange(ref _readyThreads, _readyThreads - threads);
            for (var i = 0; i < threads; i++)
                _ = _blocks[i].Set();
            _ = _allReady.WaitOne();
        }


        public static void ThreadLoop(object? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var idx = (int)id;
            while (true)
            {
                _ = Interlocked.Increment(ref _readyThreads);
                if (_readyThreads >= _threads.Length)
                    _ = _allReady.Set();

                _ = _blocks[idx].WaitOne();
                Action.Invoke(idx);
            }
        }
    }
}