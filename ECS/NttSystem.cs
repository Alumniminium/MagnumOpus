using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HerstLib.IO;
using Prometheus;
using Serilog;
using Serilog.Core;

namespace MagnumOpus.ECS
{
    public abstract class NttSystem
    {
        public static long Tick => NttWorld.Tick;
        public string Name;
        public bool IsLogging;
        public int ThreadCount;
        internal readonly ConcurrentDictionary<int, NTT> _entities = new();
        internal readonly List<NTT> _entitiesList = new();
        private readonly Gauge TimeMetricsExporter;
        private readonly Gauge NTTCountMetricsExporter;
        internal readonly Logger Logger;

        protected NttSystem(string name, int threads = 1, bool log = true)
        {
            ThreadCount = threads;
            IsLogging = log;
            Name = name;
            TimeMetricsExporter = Metrics.CreateGauge($"MAGNUMOPUS_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}", $"Tick time for {Name} in ms");
            NTTCountMetricsExporter = Metrics.CreateGauge($"MAGNUMOPUS_ECS_SYSTEM_{Name.ToUpperInvariant().Replace(" ", "_")}_NTT_COUNT", $"NTT count for {Name}");

            var logCfg = new LoggerConfiguration()
                                .Enrich.WithProperty("System", name)
                                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss}[{Properties}] {Message}{NewLine}{Exception}")
                                // .WriteTo.GrafanaLoki("http://loki.her.st")
                                .WriteTo.File($"{DateTime.UtcNow:dd-MM-yyyy}.log");
            if (IsLogging)
                logCfg.MinimumLevel.Debug();
            else
                logCfg.MinimumLevel.Information();

            Logger = logCfg.CreateLogger();
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
            ThreadedWorker.Run(EndUpdate, ThreadCount);
            NTTCountMetricsExporter.Set(_entities.Count);
            TimeMetricsExporter.Set((float)Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
        }

        public void EndUpdate(int idx, int threads)
        {
            var start = 0;
            var amount = _entitiesList.Count;

            if (amount > threads * 2)
            {
                var chunkSize = amount / threads;
                var remaining = amount % threads;
                start = (chunkSize * idx) + Math.Min(idx, remaining);
                amount = chunkSize + (idx < remaining ? 1 : 0);
            }
            else if (idx != 0)
                return;

            Update(start, amount);
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
    public abstract class NttSystem<T> : NttSystem where T : struct
    {
        protected NttSystem(string name, int threads = 1, bool log = false) : base(name, threads, log) { }
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
    public abstract class NttSystem<T, T2> : NttSystem where T : struct where T2 : struct
    {
        protected NttSystem(string name, int threads = 1, bool log = false) : base(name, threads, log) { }
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
    public abstract class NttSystem<T, T2, T3> : NttSystem where T : struct where T2 : struct where T3 : struct
    {
        protected NttSystem(string name, int threads = 1, bool log = false) : base(name, threads, log) { }
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
    public abstract class NttSystem<T, T2, T3, T4> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected NttSystem(string name, int threads = 1, bool log = false) : base(name, threads, log) { }
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
    public abstract class NttSystem<T, T2, T3, T4, T5> : NttSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected NttSystem(string name, int threads = 1, bool log = false) : base(name, threads, log) { }
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
}