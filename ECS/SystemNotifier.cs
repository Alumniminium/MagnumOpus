namespace MagnumOpus.ECS
{
    public class SystemNotifier
    {
        private readonly NttSystem[] _array;
        private readonly int _threadCount;
        private readonly Thread[] _threads;
        private readonly AutoResetEvent[] _events;
        private readonly AutoResetEvent _readyEvent;
        private int _readyThreads;

        public SystemNotifier(NttSystem[] array)
        {
            _array = array;
            _threadCount = 2;
            _threads = new Thread[_threadCount];
            _events = new AutoResetEvent[_threadCount];
            _readyEvent = new AutoResetEvent(false);
            for (var i = 0; i < _threadCount; i++)
            {
                _events[i] = new AutoResetEvent(false);
                _threads[i] = new Thread(WorkLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _threads[i].Start(i);
            }
        }

        public void Start()
        {
            Interlocked.Exchange(ref _readyThreads, 0);
            for (var i = 0; i < _threadCount; i++)b
                _events[i].Set();
            _readyEvent.WaitOne();
        }

        private void WorkLoop(object idx)
        {
            int id = (int)idx;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                if (_readyThreads == _threadCount)
                    _readyEvent.Set();

                _events[id].WaitOne();

                while (NttWorld.ChangedThisTick.TryDequeue(out var entity))
                    for (int i = 0; i < _array.Length; i++)
                        _array[i].EntityChanged(in entity);

                _readyEvent.Set();
            }
        }
    }
}
