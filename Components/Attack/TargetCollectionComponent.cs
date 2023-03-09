using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public readonly struct TargetCollectionComponent
    {
        public readonly List<NTT> Targets = new();
        public readonly MagicType.Entry MagicType;

        public TargetCollectionComponent(MagicType.Entry magicType)
        {
            MagicType = magicType;
        }
    }
}