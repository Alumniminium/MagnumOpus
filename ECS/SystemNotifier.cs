namespace MagnumOpus.ECS
{
    public class SystemNotifier
    {
        private readonly NttSystem[] _array;
        private readonly int _threadCount;
        private readonly Thread[] _threads;
        private readonly Barrier _barrier;

        public SystemNotifier(NttSystem[] array)
        {
            _array = array;
            _threadCount = 2;
            _barrier = new Barrier(_threadCount + 1);
            _threads = new Thread[_threadCount];
            for (var i = 0; i < _threadCount; i++)
            {
                _threads[i] = new Thread(WorkLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _threads[i].Start();
            }
        }

        public void Start() => _barrier.SignalAndWait();
        private void WorkLoop()
        {
            while (true)
            {
                _barrier.SignalAndWait();
                while (NttWorld.ChangedThisTick.TryDequeue(out var entity))
                {
                    PrometheusPush.NTTChanges.Inc();
                    for (var i = 0; i < _array.Length; i++)
                        _array[i].EntityChanged(in entity);
                }
            }
        }
    }
}
