using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Attack
{
    [Component]
    [Save]
    public struct AttackComponent
    {
        public readonly NTT Target;
        public readonly MsgInteractType AttackType;
        public long SleepTicks;

        [JsonConstructor]
        public AttackComponent(in NTT target, MsgInteractType attackType)
        {
            Target = target;
            AttackType = attackType;
        }
    }
}