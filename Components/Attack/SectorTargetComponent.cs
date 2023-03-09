using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public readonly struct SectorTargetComponent
    {
        public readonly MagicType.Entry MagicType;
        public readonly ushort X;
        public readonly ushort Y;

        public SectorTargetComponent(ushort x, ushort y, MagicType.Entry magicType)
        {
            X = x;
            Y = y;
            MagicType = magicType;
        }
    }
}