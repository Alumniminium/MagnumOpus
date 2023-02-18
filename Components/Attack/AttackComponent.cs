using Co2Core.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct AttackComponent
    {
        public readonly int EntityId;
        public readonly NTT Target;
        public readonly MsgInteractType AttackType;
        public long SleepTicks;

        public AttackComponent(int nttId, in NTT target, MsgInteractType attackType)
        {
            EntityId = nttId;
            Target = target;
            AttackType = attackType;
        }

        public override int GetHashCode() => EntityId;
    }
    
}