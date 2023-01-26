using System;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IdGenerator
    {
        static readonly Dictionary<EntityType, Stack<int>> Ids;
        
        static IdGenerator()
        {
            Ids = new Dictionary<EntityType, Stack<int>>
            {
                [EntityType.Npc] = new Stack<int>(Enumerable.Range(0,100_000)),
                [EntityType.Item] = new Stack<int>(Enumerable.Range(100_000, 200_000)),
                [EntityType.Monster] = new Stack<int>(Enumerable.Range(400_000, 200_000)),
                [EntityType.Player] = new Stack<int>(Enumerable.Range(1_000_000, 100_000)),
                [EntityType.Other] = new Stack<int>(Enumerable.Range(2_000_000, 1_000_000)),
            };
        }

        public static int Get(EntityType type) => Ids[type].Pop();
        public static void Return(EntityType type, int id) => Ids[type].Push(id);
    }
}