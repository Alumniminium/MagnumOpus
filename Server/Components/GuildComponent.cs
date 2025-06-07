using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Guild membership component that contains all guild-related information for a player entity.
/// Stores guild identity (ID, name, leader), member rank, financial data (donation, funds),
/// and guild member arrays. Currently defined but not actively processed by any systems -
/// represents planned guild system functionality for social organization and group management.
/// </summary>
public struct GuildComponent
{
    public int EntityId;
    public int GuildId;
    public int LeaderId;
    public string GuildName;
    public GuildRanks Rank;
    public int Donation;
    public int Funds;
    public NTT[] Members = [];

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
        Members = [];
    }

    public override readonly int GetHashCode() => EntityId;
}