using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct MagicAttackRequestComponent
    {
        public readonly int EntityId;
        public readonly int SkillId;
        public readonly int TargetId;
        public readonly ushort X;
        public readonly ushort Y;
        public int SleepTicks;

        public MagicAttackRequestComponent(int nttId, int skillId, int targetId, ushort x, ushort y, int sleepTicks)
        {
            EntityId = nttId;
            SkillId = skillId;
            TargetId = targetId;
            X = x;
            Y = y;
            SleepTicks = sleepTicks;
        }

        public override int GetHashCode() => EntityId;
    }
}