using System.Numerics;
using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using System.Collections.Concurrent;
using NttECS.ECS;
using NttECS.Memory;

namespace MagnumOpus.SpacePartitioning
{
    internal class BucketList
    {
        public readonly SwapList<NTT> Entities = new(64);
        public readonly ReaderWriterLockSlim Lock = new(LockRecursionPolicy.NoRecursion);
    }

    public class SpatialHash(int cellSize = 10)
    {
        private readonly int cellSize = cellSize;
        private readonly ConcurrentDictionary<int, BucketList> Hashtbl = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            var bucket = Hashtbl.GetOrAdd(hash, _ => new BucketList());

            bucket.Lock.EnterWriteLock();
            try
            {
                bucket.Entities.Add(entity);
            }
            finally
            {
                bucket.Lock.ExitWriteLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            if (Hashtbl.TryGetValue(hash, out var bucket))
            {
                bucket.Lock.EnterWriteLock();
                try
                {
                    bucket.Entities.Remove(entity);
                    if (bucket.Entities.Count == 0)
                        Hashtbl.TryRemove(hash, out var _);
                }
                finally
                {
                    bucket.Lock.ExitWriteLock();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(NTT ntt, PositionComponent pos)
        {
            Remove(ntt, pos.LastPosition);
            Add(ntt, pos.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            var cx = vwp.Viewport.X + (vwp.Viewport.Width / 2);
            var cy = vwp.Viewport.Y + (vwp.Viewport.Height / 2);

            var minX = vwp.Viewport.Left / cellSize;
            var maxX = vwp.Viewport.Right / cellSize;
            var minY = vwp.Viewport.Top / cellSize;
            var maxY = vwp.Viewport.Bottom / cellSize;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var hash = GetHash(new Vector2(x * cellSize, y * cellSize));

                    if (!Hashtbl.TryGetValue(hash, out var bucket))
                        continue;

                    bucket.Lock.EnterReadLock();
                    try
                    {
                        for (var i = 0; i < bucket.Entities.Count; i++)
                        {
                            ref readonly var pos = ref bucket.Entities[i].Get<PositionComponent>();

                            if (CoMath.InScreen(pos.Position.X, pos.Position.Y, cx, cy))
                                vwp.EntitiesVisible.Add(bucket.Entities[i]);
                        }
                    }
                    finally
                    {
                        bucket.Lock.ExitReadLock();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHash(Vector2 position)
        {
            var scaled = position / cellSize;
            var x = (int)scaled.X;
            var y = (int)scaled.Y;

            return (x * 73856093) ^ (y * 19349663);
        }
    }
    public class MapEntities
    {
        private readonly Dictionary<int, NTT> Entities;

        public MapEntities() => Entities = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(NTT entity, Vector2 pos)
        {
            Entities.Add(entity.Id, entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(NTT entity, Vector2 pos)
        {
            Entities.Remove(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(NTT ntt, PositionComponent pos)
        {
            Remove(ntt, pos.LastPosition);
            Add(ntt, pos.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            foreach (var kvp in Entities)
            {
                ref readonly var pos = ref kvp.Value.Get<PositionComponent>();
                var cx = vwp.Viewport.X + (vwp.Viewport.Width / 2);
                var cy = vwp.Viewport.Y + (vwp.Viewport.Height / 2);

                if (CoMath.InScreen(pos.Position.X, pos.Position.Y, cx, cy))
                    vwp.EntitiesVisible.Add(kvp.Value);
            }
        }
    }
}