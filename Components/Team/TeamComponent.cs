using System.Text.Json.Serialization;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Team
{
    [Component]
    public struct TeamComponent
    {
        public readonly int EntityId;
        public readonly long CreatedTick;
        public int MemberCount;
        public NTT[] Members = new NTT[5];
        [JsonIgnore]
        public NTT Leader => Members[0];
        public bool ShareItems;
        public bool ShareGold;

        public TeamComponent(in NTT ntt)
        {
            CreatedTick = NttWorld.Tick;
            EntityId = ntt.Id;
            Members[0] = ntt;
            MemberCount = 1;
        }

        public override int GetHashCode() => EntityId;
    }
}