using MagnumOpus.ECS;
using MagnumOpus.Enums;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct AttackComponent(in NTT target, MsgInteractType attackType)
    {
        public NTT Target = target;
        public MsgInteractType AttackType = attackType;
        public long SleepTicks;
    }
}