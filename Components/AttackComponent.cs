using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct AttackComponent
    {
        public NTT Target;
        public MsgInteractType AttackType;
        public long SleepTicks;

        public AttackComponent(in NTT target, MsgInteractType attackType)
        {
            Target = target;
            AttackType = attackType;
        }
    }
}