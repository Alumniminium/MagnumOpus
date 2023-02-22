using System.Collections.Concurrent;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.SpacePartitioning
{
    public class SpatialHash
    {
        private readonly int cellSize;
        private readonly ConcurrentDictionary<int, List<NTT>> Hashtbl;
        private readonly object _lock = new();

        public SpatialHash(int cellSize)
        {
            this.cellSize = cellSize;
            Hashtbl = new ConcurrentDictionary<int, List<NTT>>();
        }

        public void Add(in NTT entity)
        {
            ref readonly var pos = ref entity.Get<PositionComponent>();
            int hash = GetHash(pos.Position);

            if (!Hashtbl.ContainsKey(hash))
                Hashtbl[hash] = new List<NTT>();

            lock (_lock)
                Hashtbl[hash].Add(entity);
        }

        public void Remove(in NTT entity)
        {
            ref readonly var pos = ref entity.Get<PositionComponent>();
            int hash = GetHash(pos.Position);

            if (Hashtbl.TryGetValue(hash, out var bucket))
                lock (_lock)
                    bucket.Remove(entity);
        }

        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            var cx = vwp.Viewport.X + vwp.Viewport.Width / 2;
            var cy = vwp.Viewport.Y + vwp.Viewport.Height / 2;

            int minX = (int)((vwp.Viewport.Left - vwp.Viewport.Width / 2) / cellSize);
            int maxX = (int)((vwp.Viewport.Right + vwp.Viewport.Width / 2) / cellSize);
            int minY = (int)((vwp.Viewport.Top - vwp.Viewport.Height / 2) / cellSize);
            int maxY = (int)((vwp.Viewport.Bottom + vwp.Viewport.Height / 2) / cellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    int hash = GetHash(new Vector2(x * cellSize, y * cellSize));

                    if (!Hashtbl.TryGetValue(hash, out var entities))
                        continue;

                    lock (_lock)
                    {
                        foreach (var entity in entities)
                        {
                            ref readonly var pos = ref entity.Get<PositionComponent>();
                            var distanceSquared = Vector2.DistanceSquared(pos.Position, new Vector2(cx, cy));

                            if (distanceSquared <= 324f)
                                vwp.EntitiesVisible.TryAdd(entity.Id, entity);
                        }
                    }
                }
            }
        }

        private int GetHash(Vector2 position)
        {
            var scaled = position / cellSize;
            int x = (int)scaled.X;
            int y = (int)scaled.Y;

            return x * 73856093 ^ y * 19349663;
        }
    }
}