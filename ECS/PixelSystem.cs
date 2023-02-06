using System.Runtime.CompilerServices;

namespace MagnumOpus.ECS
{
    public abstract class PixelSystem
    {
        public string Name;
        internal readonly HashSet<PixelEntity> _entities = new();
        internal float deltaTime;

        public readonly ParallelOptions parallelOptions;

        protected PixelSystem(string name, int threads = 1)
        {
            Name = name;
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = threads, TaskScheduler = new PixelTaskScheduler(threads) };
            PerformanceMetrics.RegisterSystem(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime)
        {
            this.deltaTime = deltaTime;
            PreUpdate();
            Update();
            PostUpdate();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PostUpdate() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Update() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void PreUpdate() { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool MatchesFilter(in PixelEntity nttId) => nttId.Id != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EntityChanged(in PixelEntity ntt)
        {
            var isMatch = MatchesFilter(in ntt);
            if (!isMatch)
                _entities.Remove(ntt);
            else
                _entities.Add(ntt);
        }
    }
    public abstract class PixelSystem<T> : PixelSystem where T : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T>() && base.MatchesFilter(in nttId);

        protected override void Update()
        {
            //Parallel.ForEach(_entities, parallelOptions, ntt =>
            // for(var i = 0; i < _entitiesArr.Length; i++)
            // {
            //     ref var ntt = ref _entitiesArr[i];
            //     ref var c1 = ref ntt.Get<T>();
            //     Update(in ntt, ref c1);
            // }
            foreach (var ntt in _entities)
            {
                ref var c1 = ref ntt.Get<T>();
                Update(in ntt, ref c1);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in PixelEntity ntt, ref T c1);
    }
    public abstract class PixelSystem<T, T2> : PixelSystem where T : struct where T2 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2>() && base.MatchesFilter(in nttId);

        protected override void Update()
        {
            //Parallel.ForEach(_entities, parallelOptions, ntt =>
            foreach (var ntt in _entities)
            {
            // for (var i = 0; i < _entitiesArr.Length; i++)
            // {
            //     ref var ntt = ref _entitiesArr[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                Update(in ntt, ref c1, ref c2);
                //});
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2);
    }
    public abstract class PixelSystem<T, T2, T3> : PixelSystem where T : struct where T2 : struct where T3 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3>() && base.MatchesFilter(in nttId);

        protected override void Update()
        {
            //Parallel.ForEach(_entities, parallelOptions, ntt =>
            foreach (var ntt in _entities)
            {
            // for (var i = 0; i < _entitiesArr.Length; i++)
            // {
            //     ref var ntt = ref _entitiesArr[i];
                // var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                Update(in ntt, ref c1, ref c2, ref c3);
                //});
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3);
    }
    public abstract class PixelSystem<T, T2, T3, T4> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3, T4>() && base.MatchesFilter(in nttId);

        protected override void Update()
        {
            //Parallel.ForEach(_entities, parallelOptions, ntt =>
            foreach (var ntt in _entities)
            {
            // for (var i = 0; i < _entitiesArr.Length; i++)
            // {
            //     ref var ntt = ref _entitiesArr[i];
                // var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4);
                //});
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
    }
    public abstract class PixelSystem<T, T2, T3, T4, T5> : PixelSystem where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
    {
        protected PixelSystem(string name, int threads = 1) : base(name, threads) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Has<T, T2, T3, T4, T5>() && base.MatchesFilter(in nttId);

        protected override void Update()
        {
            //Parallel.ForEach(_entities, parallelOptions, ntt =>
            foreach (var ntt in _entities)
            {
            // for (var i = 0; i < _entitiesArr.Length; i++)
            // {
            //     ref var ntt = ref _entitiesArr[i];
                // var ntt = _entities[i];
                ref var c1 = ref ntt.Get<T>();
                ref var c2 = ref ntt.Get<T2>();
                ref var c3 = ref ntt.Get<T3>();
                ref var c4 = ref ntt.Get<T4>();
                ref var c5 = ref ntt.Get<T5>();
                Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
                //});
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Update(in PixelEntity ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
    }
}