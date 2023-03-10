using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.Attack
{
    [Component]
    [Save]
    public struct AttackComponent
    {
        public readonly NTT Target;
        public readonly MsgInteractType AttackType;
        public long SleepTicks;

        public AttackComponent(in NTT target, MsgInteractType attackType)
        {
            Target = target;
            AttackType = attackType;
        }
    }
}