using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
{
    public DestroySystem() : base("Destroy", threads: 1, log: false) { }

    // Handles final cleanup and destruction of entities marked for removal. The system sends
    // appropriate despawn packets to clients based on entity type (player/monster removal
    // packets or item deletion packets) before permanently destroying the entity from the world.
    // This ensures clients are notified of entity removal and prevents memory leaks.
    public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
    {
        // === SEND DESPAWN PACKETS TO CLIENTS ===
        // Notify nearby players about entity removal based on type
        if (ntt.IsPlayer() || ntt.IsMonster(guardsAreMonsters: true))
        {
            // Living entities use generic remove action
            var despawnPacket = MsgAction.RemoveEntity(ntt.Id);
            ntt.NetSync(ref despawnPacket, true);
        }
        else if (ntt.IsItem())
        {
            // Ground items use specific floor item deletion packet
            var deletePacket = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            ntt.NetSync(ref deletePacket, true);
        }

        // === DESTROY ENTITY FROM WORLD ===
        // Permanently remove entity from ECS world
        NttWorld.Destroy(ntt);

        if (IsLogging)
            FConsole.WriteLine("Destroyed entity {ntt}", ntt);
    }
}