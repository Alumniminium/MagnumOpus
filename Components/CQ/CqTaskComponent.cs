using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct CqTaskComponent
    {
        public readonly int EntityId;
        public readonly NTT Npc;
        public int[] Options;
        public byte OptionCount;

        public CqTaskComponent(int entityId, int npcId)
        {
            EntityId = entityId;
            Npc = NttWorld.GetEntityByNetId(npcId);
            Options = new int[16];
        }
        public override int GetHashCode() => EntityId;
    }
}