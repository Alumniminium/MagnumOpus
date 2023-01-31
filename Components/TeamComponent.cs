using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TeamComponent
    {
        public readonly int EntityId;
        public PixelEntity[] Members;
        public PixelEntity Leader => Members[0];
        public bool ShareItems;
        public bool ShareGold;

        public TeamComponent(in PixelEntity ntt)
        {
            EntityId = ntt.Id;
            Members = new PixelEntity[5];
            Members[0] = ntt;
        }

        public override int GetHashCode() => EntityId;
    }
}