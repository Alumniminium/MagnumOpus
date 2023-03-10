using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public  struct TargetCollectionComponent
    {
        public  List<NTT> Targets;
        public  MagicType.Entry MagicType;

        public TargetCollectionComponent() => Targets = new();
        public TargetCollectionComponent(MagicType.Entry magicType)
        {
            MagicType = magicType;
            Targets = new();
        }
    }
}