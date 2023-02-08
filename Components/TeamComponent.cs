using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TeamComponent
    {
        public readonly int EntityId;
        public int MemberCount;
        public NTT[] Members;
        public NTT Leader => Members[0];
        public bool ShareItems;
        public bool ShareGold;

        public TeamComponent(in NTT ntt)
        {
            EntityId = ntt.Id;
            Members = new NTT[5];
            Members[0] = ntt;
            MemberCount = 1;
        }

        public override int GetHashCode() => EntityId;
    }
}