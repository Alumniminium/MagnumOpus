using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MagnumOpus.Components;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public readonly struct NTT
    {
        public readonly int Id;
        public readonly EntityType Type;
        internal readonly long CreatedTick;

        [JsonConstructor]
        public NTT(int id, EntityType type)
        {
            Id = id;
            Type = type;
            CreatedTick = NttWorld.Tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Set<T, T2>(ref T t1, ref T2 t2) where T : struct where T2 : struct
        {
            SparseComponentStorage<T>.AddFor(this, ref t1);
            SparseComponentStorage<T2>.AddFor(this, ref t2);
        }
        public readonly void Set<T, T2, T3>(ref T t1, ref T2 t2, ref T3 t3) where T : struct where T2 : struct where T3 : struct
        {
            SparseComponentStorage<T>.AddFor(in this, ref t1);
            SparseComponentStorage<T2>.AddFor(in this, ref t2);
            SparseComponentStorage<T3>.AddFor(in this, ref t3);
        }

        public readonly void Set<T>(ref T t) where T : struct => SparseComponentStorage<T>.AddFor(in this, ref t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Set<T>(T component) where T : struct => SparseComponentStorage<T>.AddFor(in this, ref component);
        public readonly void Set<T>() where T : struct => SparseComponentStorage<T>.AddFor(in this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T Get<T>() where T : struct => ref SparseComponentStorage<T>.Get(this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Has<T>() where T : struct => SparseComponentStorage<T>.HasFor(in this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => Has<T, T2, T3, T4>() && Has<T5>();
        public readonly bool Has<T, T2, T3, T4, T5, T6>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct => Has<T, T2, T3, T4>() && Has<T5, T6>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(this);
        public readonly void ChangeOwner(NTT to) => ReflectionHelper.ChangeOwner(this, to);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NetSync<T>(ref T msg, bool broadcast = false, bool ignoreSelf = false) where T : unmanaged
        {
            if (broadcast && Has<ViewportComponent>())
            {
                ref readonly var vwp = ref Get<ViewportComponent>();
                foreach (var b in vwp.EntitiesVisible)
                {
                    if (b.Type != EntityType.Player)
                        continue;

                    if (!b.Has<NetworkComponent>())
                        continue;

                    if (ignoreSelf && b == this)
                        continue;

                    ref readonly var net = ref b.Get<NetworkComponent>();
                    var packet = Co2Packet.Serialize(ref msg);
                    net.SendQueue.Enqueue(packet);
                }
            }
            else if (Type == EntityType.Player && !ignoreSelf)
            {
                if (!Has<NetworkComponent>())
                    return;
                ref readonly var net = ref Get<NetworkComponent>();
                var packet = Co2Packet.Serialize(ref msg);
                net.SendQueue.Enqueue(packet);
            }
        }
        public readonly void NetSync(byte[] msg, bool broadcast = false)
        {
            if (broadcast && Has<ViewportComponent>())
            {
                ref readonly var vwp = ref Get<ViewportComponent>();
                foreach (var b in vwp.EntitiesVisible)
                {
                    if (b.Type != EntityType.Player)
                        continue;

                    if (!b.Has<NetworkComponent>())
                        continue;

                    ref readonly var net = ref b.Get<NetworkComponent>();
                    net.SendQueue.Enqueue(msg);
                }
            }
            else if (Type == EntityType.Player)
            {
                if (!Has<NetworkComponent>())
                    return;
                ref readonly var net = ref Get<NetworkComponent>();
                net.SendQueue.Enqueue(msg);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Id;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is NTT nttId && nttId.Id == Id;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in NTT a, in NTT b) => a.Id == b.Id;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in NTT a, in NTT b) => a.Id != b.Id;
        public static implicit operator int(in NTT a) => a.Id;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"NTT {Id} ({Type})";
    }
}