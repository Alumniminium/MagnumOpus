using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prometheus;

namespace MagnumOpus.ECS
{
    public abstract class NttSystem
    {
        public static long Tick => NttWorld.Tick;
        public string Name;
        public bool IsLogging;
        public int ThreadCount;
        internal readonly ConcurrentDictionary<long, NTT> _entities = new();
        internal readonly List<NTT> _entitiesList = [];
        private readonly Gauge TimeMetricsExporter;
        private readonly Gauge NTTCountMetricsExporter;

        protected NttSystem(string name, int threads = 1, bool log = true)
        {
            ThreadCount = threads;
            IsLogging = log;
            Name = name;
            TimeMetricsExporter = Metrics.CreateGauge($"WEBSOCKETSERVICE_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}", $"Tick time for {Name} in ms");
            NTTCountMetricsExporter = Metrics.CreateGauge($"WEBSOCKETSERVICE_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}_NTT_COUNT", $"NTT count for {Name}");
        }

        public void BeginUpdate()
        {
            var ts = Stopwatch.GetTimestamp();
            if (_entities.IsEmpty)
            {
                NTTCountMetricsExporter.Set(0);
                TimeMetricsExporter.Set((float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
                return;
            }

            if (ThreadCount > 1 && _entitiesList.Count > ThreadCount * 2)
            {
                ThreadedWorker.Run(EndUpdate, ThreadCount);
            }
            else
            {
                Update(0, _entitiesList.Count);
            }
            
            NTTCountMetricsExporter.Set(_entities.Count);
            TimeMetricsExporter.Set((float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
        }

        public void EndUpdate(int idx, int threads)
        {
            var totalEntities = _entitiesList.Count;
            
            // For small workloads, only thread 0 processes everything
            if (totalEntities <= threads * 2)
            {
                if (idx == 0)
                    Update(0, totalEntities);
                return;
            }

            // Calculate work distribution ensuring all entities are processed
            var baseChunkSize = totalEntities / threads;
            var extraEntities = totalEntities % threads;
            
            // First 'extraEntities' threads get one extra entity
            var chunkSize = baseChunkSize + (idx < extraEntities ? 1 : 0);
            var start = (baseChunkSize * idx) + Math.Min(idx, extraEntities);
            
            Update(start, chunkSize);
        }

        protected abstract void Update(int start, int amount);
        protected virtual bool MatchesFilter(in NTT nttId) => nttId.Id != 0;
        public void EntityChanged(in NTT ntt)
        {
            var isMatch = MatchesFilter(in ntt);
            if (!isMatch)
            {
                if (_entities.TryRemove(ntt.Id, out _))
                    _entitiesList.Remove(ntt);
            }
            else
            {
                if (_entities.TryAdd(ntt.Id, ntt))
                    _entitiesList.Add(ntt);
            }
        }
    }
    public abstract class NttSystem<T>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                Update(in ntt, ref c1);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1);
    }
    public abstract class NttSystem<T, T2>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                Update(in ntt, ref c1, ref c2);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2);
    }
    public abstract class NttSystem<T, T2, T3>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                Update(in ntt, ref c1, ref c2, ref c3);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class NttSystem<T, T2, T3, T4>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class NttSystem<T, T2, T3, T4, T5>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4, T5>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                ref var c5 = ref ntt.Get<T5>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
    public abstract class NttSystem<T, T2, T3, T4, T5, T6>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
    {
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4, T5, T6>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int amount)
        {
            var span = CollectionsMarshal.AsSpan(_entitiesList).Slice(start, amount);
            for (var i = 0; i < span.Length; i++)
            {
                ref readonly var ntt = ref span[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                ref var c5 = ref ntt.Get<T5>();
                ref var c6 = ref ntt.Get<T6>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5, ref c6);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5, ref T6 c6);
    }
}