using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct MagicAttackRequestComponent
    {
        public readonly int SkillId;
        public readonly int TargetId;
        public readonly ushort X;
        public readonly ushort Y;
        public int SleepTicks;

        public MagicAttackRequestComponent(int skillId, int targetId, ushort x, ushort y, int sleepTicks)
        {
            SkillId = skillId;
            TargetId = targetId;
            X = x;
            Y = y;
            SleepTicks = sleepTicks;
        }
    }
}