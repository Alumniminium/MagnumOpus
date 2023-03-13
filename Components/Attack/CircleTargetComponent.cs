using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct CircleTargetComponent
    {
        public MagicType.Entry MagicType;
        public ushort X;
        public ushort Y;

        public CircleTargetComponent(ushort x, ushort y, MagicType.Entry magicType)
        {
            X = x;
            Y = y;
            MagicType = magicType;
        }
    }

}