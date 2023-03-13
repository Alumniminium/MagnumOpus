using Co2Core.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct SectorTargetComponent
    {
        public MagicType.Entry MagicType;
        public ushort X;
        public ushort Y;

        public SectorTargetComponent(ushort x, ushort y, MagicType.Entry magicType)
        {
            X = x;
            Y = y;
            MagicType = magicType;
        }
    }
}