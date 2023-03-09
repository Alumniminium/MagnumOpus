using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public readonly struct TargetCollectionComponent
    {
        public readonly int EntityId;
        public readonly List<NTT> Targets = new();
        public readonly MagicType.Entry MagicType;

        public TargetCollectionComponent(int nttId, MagicType.Entry magicType)
        {
            EntityId = nttId;
            MagicType = magicType;
        }

        public override int GetHashCode() => EntityId;
    }

}