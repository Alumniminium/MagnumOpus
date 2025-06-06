using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct MagicAttackRequestComponent(int skillId, int targetId, ushort x, ushort y, int sleepTicks)
    {
        public int SkillId = skillId;
        public int TargetId = targetId;
        public ushort X = x;
        public ushort Y = y;
        public int SleepTicks = sleepTicks;
    }
}