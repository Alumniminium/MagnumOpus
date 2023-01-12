using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TargetCollectionComponent
    {
        public readonly int EntityId;
        public readonly List<PixelEntity> Targets;
        public readonly MagicType.Entry MagicType;

        public TargetCollectionComponent(int nttId, MagicType.Entry magicType)
        {
            EntityId = nttId;
            MagicType = magicType;
            Targets = new List<PixelEntity>();
        }
        
        public override int GetHashCode() => EntityId;
    }
    
}