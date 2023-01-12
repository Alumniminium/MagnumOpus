using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{

    [Component]
    public struct CircleTargetComponent
    {
        public readonly int EntityId;
        public readonly MagicType.Entry MagicType;
        public readonly ushort X;
        public readonly ushort Y;

        public CircleTargetComponent(int nttId, ushort x, ushort y, MagicType.Entry magicType)
        {
            EntityId = nttId;
            X = x;
            Y = y;
            MagicType = magicType;
        }

        public override int GetHashCode() => EntityId;
    }

}