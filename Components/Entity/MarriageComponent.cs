using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct MarriageComponent
    {
        public readonly int EntityId;
        public int SpouseId;
        public int WeddingTick;
        public int DivorceTick;

        public MarriageComponent(int entityId, int spouseId, int weddingTick, int divorceTick)
        {
            EntityId = entityId;
            SpouseId = spouseId;
            WeddingTick = weddingTick;
            DivorceTick = divorceTick;
        }

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}