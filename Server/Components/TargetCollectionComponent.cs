using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TargetCollectionComponent
    {
        public List<NTT> Targets;
        public MagicType.Entry MagicType;

        public TargetCollectionComponent() => Targets = [];
        public TargetCollectionComponent(MagicType.Entry magicType)
        {
            MagicType = magicType;
            Targets = [];
        }
    }
}