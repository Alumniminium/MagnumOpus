using System.Numerics;
using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using HerstLib.Memory;
using System.Collections.Concurrent;

namespace MagnumOpus.SpacePartitioning
{
    public class SpatialHash(int cellSize = 10)
    {
        private const float VISIBILITY_DISTANCE_SQUARED = 350f;
        private readonly int cellSize = cellSize;
        private readonly ConcurrentDictionary<int, List<NTT>> Hashtbl = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            if (!Hashtbl.TryGetValue(hash, out var list))
            {
                list = [];
                Hashtbl.TryAdd(hash, list);
            }
            list.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            if (Hashtbl.TryGetValue(hash, out var bucket))
            {
                bucket.Remove(entity);
                if (bucket.Count == 0)
                    Hashtbl.TryRemove(hash, out var _);
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

                    if (!Hashtbl.TryGetValue(hash, out var entities))
                        continue;

                    foreach (var ntt in entities)
                    {
                        if (vwp.EntitiesVisible.Contains(ntt))
                            continue;

                        ref readonly var pos = ref ntt.Get<PositionComponent>();
                        var distanceSquared = Vector2.DistanceSquared(pos.Position, new Vector2(cx, cy));

                        if (distanceSquared <= VISIBILITY_DISTANCE_SQUARED)
                            vwp.EntitiesVisible.Add(ntt);
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

            return Math.Abs((x * 73856093) ^ (y * 19349663));
        }
    }
    public class MapEntities
    {
        private readonly Dictionary<int, NTT> Entities;

        public MapEntities() => Entities = new Dictionary<int, NTT>();

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
                var distanceSquared = Vector2.DistanceSquared(pos.Position, new Vector2(vwp.Viewport.X, vwp.Viewport.Y));

                if (distanceSquared <= 324f)
                    vwp.EntitiesVisible.Add(kvp.Value);
            }
        }
    }
}