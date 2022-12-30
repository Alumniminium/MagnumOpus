using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct AttackComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Target;
        public readonly MsgInteractType AttackType;
        public uint SleepTicks;

        public AttackComponent(int nttId, in PixelEntity target, MsgInteractType attackType)
        {
            EntityId = nttId;
            Target = target;
            AttackType = attackType;
        }

        public override int GetHashCode() => EntityId;
    }
}