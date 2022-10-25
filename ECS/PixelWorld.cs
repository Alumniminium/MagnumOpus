using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public static class PixelWorld
    {
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        public const int MaxEntities = 1500_000;
        private static readonly PixelEntity[] Entities;
        public static PixelSystem[] Systems;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Stack<PixelEntity> ToBeRemoved = new();
        public static readonly List<PixelEntity> Players = new();
        public static readonly HashSet<PixelEntity> ChangedThisTick = new();

        static PixelWorld()
        {
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
            Systems = Array.Empty<PixelSystem>();
        }

        public static ref PixelEntity CreateEntity(EntityType type, int parentId = -1)
        {
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                Entities[arrayIndex] = new PixelEntity(arrayIndex, type, parentId);
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        public static ref PixelEntity GetEntity(int nttId)
        {
            return ref Entities[nttId];
        }

        public static bool EntityExists(int nttId)
        {
            return Entities[nttId].Id == nttId;
        }

        public static bool EntityExists(in PixelEntity ntt)
        {
            return Entities[ntt.Id].Id == ntt.Id;
        }

        public static void InformChangesFor(in PixelEntity ntt)
        {
            ChangedThisTick.Add(ntt);
        }

        public static void Destroy(in PixelEntity ntt)
        {
            ToBeRemoved.Push(ntt);
        }

        private static void DestroyInternal(in PixelEntity ntt)
        {
            foreach (var child in ntt.Children)
                DestroyInternal(child);

            AvailableArrayIndicies.Push(ntt.Id);
            Players.Remove(ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);
            ntt.Recycle();
            Entities[ntt.Id] = default;

            for (int i = 0; i < Systems.Length; i++)
                Systems[i].EntityChanged(in ntt);
        }
        public static void Update()
        {
            while (ToBeRemoved.Count != 0)
                DestroyInternal(ToBeRemoved.Pop());
            foreach (var ntt in ChangedThisTick)
            {
                for (int i = 0; i < Systems.Length; i++)
                    Systems[i].EntityChanged(in ntt);
            }
            ChangedThisTick.Clear();
        }
    }
}