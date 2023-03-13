using MagnumOpus.ECS;
namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public struct CqTaskComponent
    {
        public NTT Npc;
        public int[] Options = new int[16];
        public byte OptionCount;

        public CqTaskComponent() => Options = new int[16];
        public CqTaskComponent(int npcId) => Npc = NttWorld.GetEntity(npcId);
    }
}