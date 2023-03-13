using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct MagicAttackRequestComponent
    {
        public int SkillId;
        public int TargetId;
        public ushort X;
        public ushort Y;
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