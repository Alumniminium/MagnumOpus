using System.Collections.Concurrent;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IdGenerator
    {
        private static readonly Dictionary<EntityType, ConcurrentStack<int>> Ids;

        static IdGenerator()
        {
            Ids = new Dictionary<EntityType, ConcurrentStack<int>>
            {
                [EntityType.Npc] = new(Enumerable.Range(0, 100_000)),
                [EntityType.Item] = new(Enumerable.Range(100_000, 299_999)),
                [EntityType.Monster] = new(Enumerable.Range(400_000, 399_999)),
                [EntityType.Player] = new(Enumerable.Range(1_000_000, 100_000)),
                [EntityType.Other] = new(Enumerable.Range(2_000_000, 1_000_000)),
                [EntityType.InvItem] = new(Enumerable.Range(3_000_000, 1_000_000)),
            };
        }

        public static int Get(EntityType type) => Ids[type].TryPop(out var id) ? id : throw new IndexOutOfRangeException(type.ToString());
        public static void Return(EntityType type, int id) => Ids[type].Push(id);
    }
}