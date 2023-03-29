using Co2Core.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct TargetingComponent
    {
        public MagicType.Entry MagicType;
        public ushort X;
        public ushort Y;
        public TargetingType TargetingType;

        public TargetingComponent(ushort x, ushort y, MagicType.Entry magicType, TargetingType targetingType)
        {
            X = x;
            Y = y;
            MagicType = magicType;
            TargetingType = targetingType;
        }
    }
}