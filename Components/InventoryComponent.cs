using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct InventoryComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public uint Money;
        public uint CPs;

        public readonly PixelEntity[] Items;

        public InventoryComponent(int entityId, uint money, uint cps)
        {
            EntityId = entityId;
            ChangedTick = ConquerWorld.Tick;
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