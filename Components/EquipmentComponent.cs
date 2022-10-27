using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public int Head;
        public int Armor;
        public int MainHand;
        public int OffHand;
        public int Boots;
        public int Garment;

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Head = 0;
            Armor = 0;
            MainHand = 0;
            OffHand = 0;
            Boots = 0;
            Garment = 0;
        }

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}