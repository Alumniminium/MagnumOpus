using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public PixelEntity Head;
        public PixelEntity Necklace;
        public PixelEntity Garment;
        public PixelEntity Armor;
        public PixelEntity Ring;
        public PixelEntity MainHand;
        public PixelEntity OffHand;
        public PixelEntity Boots;

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}