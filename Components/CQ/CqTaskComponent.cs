using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public struct CqTaskComponent
    {
        public readonly NTT Npc;
        public int[] Options = new int[16];
        public byte OptionCount;

        [JsonConstructor]
        public CqTaskComponent(int npcId) => Npc = NttWorld.GetEntity(npcId);
    }
}