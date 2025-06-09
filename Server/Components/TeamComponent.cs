using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Player party/team component managing group membership and shared resources. Contains team
/// creation time, member count, member array (max 5), leader designation, and item/gold sharing
/// settings. Not saved to database (no SaveEnabled). Used by TeamSystem for experience distribution,
/// team management, level validation, and party mechanics. Essential for cooperative gameplay,
/// experience sharing, and group-based activities.
/// </summary>
public struct TeamComponent
{
    public long CreatedTick;
    public int MemberCount;
    public NTT[] Members = new NTT[5];
    public readonly NTT Leader => Members[0];
    public bool ShareItems;
    public bool ShareGold;

    public TeamComponent(NTT ntt)
    {
        CreatedTick = NttWorld.Tick;
        Members[0] = ntt;
        MemberCount = 1;
        ShareItems = true;
        ShareGold = true;
    }
}
