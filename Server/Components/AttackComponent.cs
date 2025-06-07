using MagnumOpus.Enums;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// This component triggers attacks on entities. 
    /// It contains the target entity and the type of attack to perform.
    /// It automatically gets removed if the attacker moves, dies or the target
    /// is out of range or has been killed.
    /// </summary>
    public struct AttackComponent(in NTT target, MsgInteractType attackType)
    {
        public NTT Target = target;
        public MsgInteractType AttackType = attackType;
        public long CooldownTicks;
    }
}