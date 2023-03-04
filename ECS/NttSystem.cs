using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Grafana.Loki;

namespace MagnumOpus.ECS
{
    public abstract class NttSystem
    {
        public static long Tick => NttWorld.Tick;
        public string Name;
        public bool Trace = false;
        internal readonly ConcurrentDictionary<int, NTT> _entities = new();
        internal readonly List<NTT> _entitiesList = new();
        internal readonly Thread[] _threads;
        internal readonly AutoResetEvent[] _blocks;
        private readonly AutoResetEvent _allReady = new(false);
        internal volatile int _readyThreads = 0;
        private readonly Gauge TimeMetricsExporter;
        private readonly Gauge NTTCountMetricsExporter;

        internal readonly Logger Logger;

        protected NttSystem(string name, int threads = 1)
        {
            Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .Enrich.WithProperty("System", name)
                            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}[{Properties}] {Message}{NewLine}{Exception}")
                            .WriteTo.GrafanaLoki("http://loki.her.st")
                            .CreateLogger();

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
            TimeMetricsExporter = Metrics.CreateGauge($"MAGNUMOPUS_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}", $"Tick time for {Name} in ms");
            NTTCountMetricsExporter = Metrics.CreateGauge($"MAGNUMOPUS_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}_NTT_COUNT", $"NTT count for {Name}");
        }

        public void BeginUpdate()
        {
            var ts = Stopwatch.GetTimestamp();
            if (_entities.IsEmpty)
            {
                PerformanceMetrics.AddSample(Name, (float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
                NTTCountMetricsExporter.Set(0);
                TimeMetricsExporter.Set((float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
                return;
            }
            _allReady.Reset();
            Interlocked.Exchange(ref _readyThreads, 0);
            for (int i = 0; i < _threads.Length; i++)
                _blocks[i].Set();
            _allReady.WaitOne();
            PerformanceMetrics.AddSample(Name, (float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
            TimeMetricsExporter.Set((float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
            NTTCountMetricsExporter.Set(_entities.Count);
        }

        public void ThreadLoop(object? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            int idx = (int)id;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                if (_readyThreads >= _threads.Length)
                    _allReady.Set();

                _blocks[idx].WaitOne();

                int start = 0;
                int amount = _entitiesList.Count;

                if (_entitiesList.Count >= _threads.Length)
                {
                    int chunkSize = _entitiesList.Count / _threads.Length;
                    int remaining = _entitiesList.Count % _threads.Length;
                    start = chunkSize * idx + Math.Min(idx, remaining);
                    amount = chunkSize + (idx < remaining ? 1 : 0);
                }
                else if (idx != 0)
                    continue;

                Update(start, start + amount);
            }
        }

        protected abstract void Update(int start, int end);
        protected virtual bool MatchesFilter(in NTT nttId) => nttId.Id != 0;
        internal void EntityChanged(in NTT ntt)
        {
            var isMatch = MatchesFilter(in ntt);
            if (!isMatch)
            {
                if (_entities.TryRemove(ntt.Id, out _))
                    lock (_entitiesList)
                        _entitiesList.Remove(ntt);
            }
            else
            {
                if (_entities.TryAdd(ntt.Id, ntt))
                    lock (_entitiesList)
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
            for (int i = start; i < Math.Min(end, _entitiesList.Count); i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                Update(in ntt, ref c1);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1);
    }
    public abstract class NttSystem<T, T2> : NttSystem where T : struct where T2 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < Math.Min(end, _entitiesList.Count); i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                Update(in ntt, ref c1, ref c2);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2);
    }
    public abstract class NttSystem<T, T2, T3> : NttSystem where T : struct where T2 : struct where T3 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < Math.Min(end, _entitiesList.Count); i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                Update(in ntt, ref c1, ref c2, ref c3);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class NttSystem<T, T2, T3, T4> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < Math.Min(end, _entitiesList.Count); i++)
            {
                NTT ntt = _entitiesList[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4);
            }
        }
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class NttSystem<T, T2, T3, T4, T5> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected NttSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in NTT nttId) => nttId.Has<T, T2, T3, T4, T5>() && base.MatchesFilter(in nttId);

        protected override void Update(int start, int end)
        {
            for (int i = start; i < Math.Min(end, _entitiesList.Count); i++)
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
        public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
}