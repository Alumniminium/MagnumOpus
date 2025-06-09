using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using MagnumOpus.Networking;
using NttECS.ECS;

namespace MagnumOpus.Helpers;

public static class NttExtension
{
    /// <summary>
    /// Checks if the entity has a DeadTagComponent
    /// </summary>
    public static bool IsDead(this NTT ntt) => ntt.Has<DeathTagComponent>();
    /// <summary>
    /// Checks if the entity has a DeadTagComponent
    /// </summary>
    public static bool IsAlive(this NTT ntt) => !ntt.IsDead();

    /// <summary>
    /// Checks if the entity has a SpawnerComponent
    /// </summary>
    public static bool NotSpawner(this NTT ntt) => !ntt.IsSpawner();
    /// <summary>
    /// Checks if the entity has a PlayerComponent
    /// </summary>
    public static bool NotPlayer(this NTT ntt) => !ntt.IsPlayer();
    /// <summary>
    /// Checks if the entity has a NpcComponent
    /// </summary>
    public static bool NotNpc(this NTT ntt) => !ntt.IsNpc();
    /// <summary>
    /// Checks if the entity has a MonsterComponent
    /// Optionally filters out Guards
    /// </summary>
    public static bool NotMonster(this NTT ntt, bool guardsAreMonsters) => !ntt.IsMonster(guardsAreMonsters);
    /// <summary>
    /// Checks if the entity has an ItemComponent
    /// </summary>
    public static bool NotItem(this NTT ntt) => !ntt.IsItem();
    /// <summary>
    /// Checks if the entity has a TrapComponent
    /// </summary>
    public static bool NotTrap(this NTT ntt) => !ntt.IsTrap();
    /// <summary>
    /// Checks if the entity has a GuardPositionComponent
    /// </summary>
    public static bool NotGuard(this NTT ntt) => !ntt.IsGuard();
    /// <summary>
    /// Checks if the entity has a GuardPositionComponent
    /// </summary>
    public static bool IsGuard(this NTT ntt) => ntt.Has<GuardPositionComponent>();
    /// <summary>
    /// Checks if the entity has a SpawnerComponent
    /// </summary>
    public static bool IsSpawner(this NTT ntt) => ntt.Has<SpawnerComponent>();
    /// <summary>
    /// Checks if the entity has a PlayerComponent
    /// </summary>
    public static bool IsPlayer(this NTT ntt) => ntt.Has<NetworkComponent>() || ntt.Has<PlayerComponent>();
    /// <summary>
    /// Checks if the entity has a NpcComponent
    /// </summary>  
    public static bool IsNpc(this NTT ntt) => ntt.Has<NpcComponent>();
    /// <summary>
    /// Checks if the entity has a MonsterComponent
    /// Optionally filters out Guards
    /// </summary>
    public static bool IsMonster(this NTT ntt, bool guardsAreMonsters) => ntt.Has<CqMonsterComponent>() || (guardsAreMonsters && ntt.Has<GuardPositionComponent>());
    /// <summary>
    /// Checks if the entity has an ItemComponent
    /// </summary>
    public static bool IsItem(this NTT ntt) => ntt.Has<ItemComponent>();
    /// <summary>
    /// Checks if the entity has a TrapComponent
    /// </summary>
    public static bool IsTrap(this NTT ntt) => ntt.Has<TrapComponent>();

    /// <summary>
    /// Synchronizes a network message to this entity or broadcasts it to nearby players.
    /// Handles both direct player messaging and area-of-effect broadcasting based on viewport.
    /// </summary>
    /// <typeparam name="T">Network message type (must be unmanaged)</typeparam>
    /// <param name="msg">Message to synchronize</param>
    /// <param name="broadcast">Whether to broadcast to all visible entities</param>
    /// <param name="ignoreSelf">Whether to exclude this entity from broadcast</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NetSync<T>(this NTT me, ref T msg, bool broadcast = false, bool ignoreSelf = false) where T : unmanaged
    {
        var packet = Co2Packet.Serialize(ref msg);
        NetSync(me, packet, broadcast, ignoreSelf);
    }

    public static void NetSync(this NTT me, byte[] msg, bool broadcast, bool ignoreSelf)
    {
        if (broadcast && me.Has<ViewportComponent>())
        {
            ref readonly var vwp = ref me.Get<ViewportComponent>();
            foreach (var b in vwp.EntitiesVisible)
            {
                if (b.NotPlayer() || b == me)
                    continue;

                if (b == me)
                    continue;

                ref readonly var net = ref b.Get<NetworkComponent>();
                net.SendQueue.Enqueue(msg);
            }
        }
        if (me.IsPlayer() && !ignoreSelf)
        {
            ref readonly var net = ref me.Get<NetworkComponent>();
            net.SendQueue.Enqueue(msg);
        }
    }

    /// <summary>
    /// Synchronizes a pre-serialized network message to this entity or broadcasts it to nearby players.
    /// </summary>
    /// <param name="msg">Pre-serialized message bytes</param>
    /// <param name="broadcast">Whether to broadcast to all visible entities</param>
    public static void NetSendSelf(this NTT ntt, byte[] msg)
    {
        if (!ntt.Has<NetworkComponent>())
            return;
        ref readonly var net = ref ntt.Get<NetworkComponent>();
        net.SendQueue.Enqueue(msg);
    }
}