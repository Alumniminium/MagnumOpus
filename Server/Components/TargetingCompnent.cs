using Co2Core.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct TargetingComponent(ushort x, ushort y, MagicType.Entry magicType, TargetingType targetingType)
    {
        public MagicType.Entry MagicType = magicType;
        public ushort X = x;
        public ushort Y = y;
        public TargetingType TargetingType = targetingType;
    }
}