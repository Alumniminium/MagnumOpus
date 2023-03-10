using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.Guild
{
    [Component]
    [Save]
    public struct GuildComponent
    {
        public readonly int EntityId;
        public readonly int GuildId;
        public readonly int LeaderId;
        public readonly string GuildName;
        public readonly string GuildTag;
        public readonly GuildRanks Rank;
        public int Donation;
        public int Funds;
        public NTT[] Members = Array.Empty<NTT>();

        public GuildComponent(int entityId, int guildId, int leaderId, string guildName, string guildTag, int donation, int funds, GuildRanks guildRank)
        {
            EntityId = entityId;
            GuildId = guildId;
            GuildName = guildName;
            GuildTag = guildTag;
            Rank = guildRank;
            LeaderId = leaderId;
            Donation = donation;
            Funds = funds;
            Members = Array.Empty<NTT>();
        }

        public override int GetHashCode() => EntityId;
    }
}