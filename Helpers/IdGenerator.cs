using System;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IdGenerator
    {
        public static Dictionary<EntityType, Stack<int>> Ids;
        
        static IdGenerator()
        {
            Ids = new Dictionary<EntityType, Stack<int>>
            {
                [EntityType.Player] = new Stack<int>(Enumerable.Range(1_000_000, 1_100_000)),
                [EntityType.Monster] = new Stack<int>(Enumerable.Range(400_000,500_000)),
                [EntityType.Npc] = new Stack<int>(Enumerable.Range(0,100_000)),
                [EntityType.Trap] = new Stack<int>(Enumerable.Range(200_000,300_000)),
                [EntityType.Item] = new Stack<int>(Enumerable.Range(0,200_000)),
            };
        }

        public static int Get(EntityType type) => Ids[type].Pop();
        public static void Return(EntityType type, int id) => Ids[type].Push(id);
    }
}