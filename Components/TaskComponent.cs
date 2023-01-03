using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TaskComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Npc;
        public int[] Options;
        public byte OptionCount;

        public TaskComponent(int entityId, int npcId)
        {
            EntityId = entityId;
            Npc = PixelWorld.GetEntityByNetId(npcId);
            Options = new int[16];
        }
        public override int GetHashCode() => EntityId;
    }
}