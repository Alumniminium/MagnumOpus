using System.Runtime.CompilerServices;
using HerstLib.IO;

namespace MagnumOpus.ECS
{
    public abstract class NttSystem
    {
        public string Name;
        internal readonly HashSet<NTT> _entities = new();
        internal readonly List<NTT> _entitiesList = new();
        internal readonly Thread[] _threads;
        internal readonly AutoResetEvent[] _blocks;
        internal volatile int _readyThreads =0;

        protected NttSystem(string name, int threads = 1)
        {
            Name = name;
            PerformanceMetrics.RegisterSystem(this);
            _threads = new Thread[threads];
            _blocks = new AutoResetEvent[threads];
            for (var i = 0; i < threads; i++)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginUpdate()
        {
            for(int i = 0; i < _threads.Length; i++)
                _blocks[i].Set();

            while (_readyThreads != _threads.Length)
                continue;
        }
        public void ThreadLoop(object id)
        {
            int idx = (int)id;
            while (true)
            {
                FConsole.WriteLine($"[{Name}] Thread {idx} waiting");
                Interlocked.Increment(ref _readyThreads);
                _blocks[idx].WaitOne();
                FConsole.WriteLine($"[{Name}] Thread {idx} running");
                Interlocked.Decrement(ref _readyThreads);

                int start = 0;
                int amount = _entitiesList.Count;
                
                if (_threads.Length > 1)
                {
                    amount = _entitiesList.Count / _threads.Length;
                    start = amount * idx;
                    if (idx == _threads.Length)
                        amount += _entitiesList.Count % _threads.Length;

                    amount = Math.Min(amount, _entitiesList.Count - start);
                }
                if(amount != 0)
                Update(start, amount);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Update(int start,int end) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool MatchesFilter(in NTT nttId) => nttId.Id != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EntityChanged(in NTT ntt)
        {
            var isMatch = MatchesFilter(in ntt);
            if (!isMatch)
            {
                if(_entities.Remove(ntt))
                    _entitiesList.Remove(ntt);
            }
            else
            {
                if(_entities.Add(ntt))
                    _entitiesList.Add(ntt);
            }
        }
    }
    public abstract class NttSystem<T> : NttSystem where T : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                Update(in ntt, ref c1);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in NTT ntt, ref T c1);
    }
    public abstract class NttSystem<T, T2> : NttSystem where T : struct where T2 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                Update(in ntt, ref c1, ref c2);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2);
    }
    public abstract class NttSystem<T, T2, T3> : NttSystem where T : struct where T2 : struct where T3 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                Update(in ntt, ref c1, ref c2, ref c3);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class NttSystem<T, T2, T3, T4> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class NttSystem<T, T2, T3, T4, T5> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4, T5>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                ref var c5 = ref ntt.Get<T5>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
}