using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct AttackComponent(in NTT target, MsgInteractType attackType)
    {
        public NTT Target = target;
        public MsgInteractType AttackType = attackType;
        public long SleepTicks;
    }
}