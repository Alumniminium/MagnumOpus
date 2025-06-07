using Co2Core.IO;
using MagnumOpus.Enums;
using NttECS.ECS;

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