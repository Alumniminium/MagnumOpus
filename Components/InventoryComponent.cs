using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct InventoryComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public uint Money;
        public uint CPs;

        public PixelEntity[] Items;

        public InventoryComponent(int entityId, uint money, uint cps)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Money = money;
            CPs = cps;
            Items = new PixelEntity[40];
        }

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}