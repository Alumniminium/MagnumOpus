using MagnumOpus.ECS;

namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public struct CqTaskComponent
    {
        public readonly int EntityId;
        public readonly NTT Npc;
        public int[] Options = new int[16];
        public byte OptionCount;

        public CqTaskComponent(int entityId, int npcId)
        {
            EntityId = entityId;
            Npc = NttWorld.GetEntity(npcId);
        }
        public override int GetHashCode() => EntityId;
    }
}