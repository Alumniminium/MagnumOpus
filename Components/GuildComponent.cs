using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct GuildComponent
    {
        public int EntityId;
        public int GuildId;
        public int LeaderId;
        public string GuildName;
        public GuildRanks Rank;
        public int Donation;
        public int Funds;
        public NTT[] Members = Array.Empty<NTT>();

        public GuildComponent()
        {
            GuildName = "Default Guild";
            Members = new NTT[1];
        }

        public GuildComponent(int entityId, int guildId, int leaderId, string guildName, int donation, int funds, GuildRanks guildRank)
        {
            EntityId = entityId;
            GuildId = guildId;
            GuildName = guildName;
            Rank = guildRank;
            LeaderId = leaderId;
            Donation = donation;
            Funds = funds;
            Members = Array.Empty<NTT>();
        }

        public override int GetHashCode() => EntityId;
    }
}