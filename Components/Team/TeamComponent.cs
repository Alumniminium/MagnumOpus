using System.Text.Json.Serialization;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Team
{
    [Component]
    public struct TeamComponent
    {
        public long CreatedTick;
        public int MemberCount;
        public NTT[] Members = new NTT[5];
        public NTT Leader => Members[0];
        public bool ShareItems;
        public bool ShareGold;

        public TeamComponent()
        {
            Members = new NTT[5];
            CreatedTick = NttWorld.Tick;
        }
        public TeamComponent(in NTT ntt)
        {
            CreatedTick = NttWorld.Tick;
            Members[0] = ntt;
            MemberCount = 1;
        }
    }
}