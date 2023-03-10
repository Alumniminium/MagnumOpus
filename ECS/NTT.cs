using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MagnumOpus.Components;
using MagnumOpus.Components.Entity;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public readonly struct NTT
    {
        public readonly int Id;
        public readonly EntityType Type;

        [JsonConstructor]
        public NTT(int id, EntityType type)
        {
            Id = id;
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Set<T>(ref T component) where T : struct => SparseComponentStorage<T>.AddFor(in this, ref component);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Set<T>(T component) where T : struct => SparseComponentStorage<T>.AddFor(in this, ref component);
        public readonly void Set<T>() where T : struct => SparseComponentStorage<T>.AddFor(in this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T Get<T>() where T : struct => ref SparseComponentStorage<T>.Get(in this);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NetSync<T>(ref T msg, bool broadcast = false) where T : unmanaged
        {
            if (broadcast && Has<ViewportComponent>())
            {
                ref readonly var vwp = ref Get<ViewportComponent>();

                foreach (var kvp in vwp.EntitiesVisible)
                {
                    var b = kvp.Value;

                    if (b.Type != EntityType.Player)
                        continue;

                    ref readonly var net = ref b.Get<NetworkComponent>();
                    var packet = Co2Packet.Serialize(ref msg);
                    net.SendQueue?.Enqueue(packet);
                }
            }
            else if (Type == EntityType.Player)
            {
                ref readonly var net = ref Get<NetworkComponent>();
                var packet = Co2Packet.Serialize(ref msg);
                net.SendQueue?.Enqueue(packet);
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