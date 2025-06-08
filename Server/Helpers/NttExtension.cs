using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using MagnumOpus.Networking;
using NttECS.ECS;

namespace MagnumOpus.Helpers;

public static class NttExtension
{
    public static bool IsDead(this NTT ntt) => ntt.Has<DeathTagComponent>();
    public static bool IsAlive(this NTT ntt) => !ntt.IsDead();
    public static bool NotPlayer(this NTT ntt) => !ntt.IsPlayer();
    public static bool NotNpc(this NTT ntt) => !ntt.IsNpc();
    public static bool NotMonster(this NTT ntt) => !ntt.IsMonster();
    public static bool NotItem(this NTT ntt) => !ntt.IsItem();
    public static bool NotTrap(this NTT ntt) => !ntt.IsTrap();

    public static bool IsPlayer(this NTT ntt) => ntt.Has<NetworkComponent>() || ntt.Has<PlayerComponent>();
    public static bool IsNpc(this NTT ntt) => ntt.Has<NpcComponent>();
    public static bool IsMonster(this NTT ntt) => ntt.Has<CqMonsterComponent>();
    public static bool IsItem(this NTT ntt) => ntt.Has<ItemComponent>();
    public static bool IsTrap(this NTT ntt) => ntt.Has<TrapComponent>();
    public static bool IsOther(this NTT ntt) => false; // No entities should match this for now
    
    /// <summary>
    /// Synchronizes a network message to this entity or broadcasts it to nearby players.
    /// Handles both direct player messaging and area-of-effect broadcasting based on viewport.
    /// </summary>
    /// <typeparam name="T">Network message type (must be unmanaged)</typeparam>
    /// <param name="msg">Message to synchronize</param>
    /// <param name="broadcast">Whether to broadcast to all visible entities</param>
    /// <param name="ignoreSelf">Whether to exclude this entity from broadcast</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NetSync<T>(this NTT ntt, ref T msg, bool broadcast = false, bool ignoreSelf = false) where T : unmanaged
    {
        if (broadcast && ntt.Has<ViewportComponent>())
        {
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            foreach (var b in vwp.EntitiesVisible)
            {
                if (!b.IsPlayer())
                    continue;

                if (!b.Has<NetworkComponent>())
                    continue;

                if (ignoreSelf && b == ntt)
                    continue;

                ref readonly var net = ref b.Get<NetworkComponent>();
                var packet = Co2Packet.Serialize(ref msg);
                net.SendQueue.Enqueue(packet);
            }
        }
        else if (ntt.IsPlayer() && !ignoreSelf)
        {
            if (!ntt.Has<NetworkComponent>())
                return;
            ref readonly var net = ref ntt.Get<NetworkComponent>();
            var packet = Co2Packet.Serialize(ref msg);
            net.SendQueue.Enqueue(packet);
        }
    }
    /// <summary>
    /// Synchronizes a pre-serialized network message to this entity or broadcasts it to nearby players.
    /// </summary>
    /// <param name="msg">Pre-serialized message bytes</param>
    /// <param name="broadcast">Whether to broadcast to all visible entities</param>
    public static void NetSync(this NTT ntt, byte[] msg, bool broadcast = false)
    {
        if (broadcast && ntt.Has<ViewportComponent>())
        {
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            foreach (var b in vwp.EntitiesVisible)
            {
                if (!b.Has<NetworkComponent>())
                    continue;

                ref readonly var net = ref b.Get<NetworkComponent>();
                net.SendQueue.Enqueue(msg);
            }
        }
        else if (ntt.IsPlayer())
        {
            if (!ntt.Has<NetworkComponent>())
                return;
            ref readonly var net = ref ntt.Get<NetworkComponent>();
            net.SendQueue.Enqueue(msg);
        }
    }
}