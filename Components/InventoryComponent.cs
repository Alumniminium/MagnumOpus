using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct InventoryComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public uint Money;
        public uint CPs;

        public NTT[] Items;

        public InventoryComponent(int entityId, uint money, uint cps)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Money = money;
            CPs = cps;
            Items = new NTT[40];
        }

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}