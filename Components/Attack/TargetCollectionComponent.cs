using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct TargetCollectionComponent
    {
        public readonly int EntityId;
        public readonly List<NTT> Targets;
        public readonly MagicType.Entry MagicType;

        public TargetCollectionComponent(int nttId, MagicType.Entry magicType)
        {
            EntityId = nttId;
            MagicType = magicType;
            Targets = new List<NTT>();
        }
        
        public override int GetHashCode() => EntityId;
    }
    
}