using System.Diagnostics;
using MagnumOpus.Components;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public readonly struct NTT
    {
        public readonly int Id;
        public readonly int NetId;
        public readonly EntityType Type;

        public NTT(int id, int netId, EntityType type)
        {
            Id = id;
            NetId = netId;
            Type = type;
        }

        public readonly void Set<T>(ref T component) where T : struct => ComponentList<T>.AddFor(in this, ref component);
        public readonly void Set<T>(T component) where T : struct => ComponentList<T>.AddFor(in this, ref component);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(this);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(in this);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => Has<T, T2, T3, T4>() && Has<T5>();
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(in this);

        public readonly void NetSync<T>(ref T msg, bool broadcast = false) where T : unmanaged
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

                    var packet = Co2Packet.Serialize(ref msg);
                    if (packet.Length == 0)
                        Debugger.Break();
                    ref var net = ref b.Get<NetworkComponent>();
                    var copy = new byte[packet.Length];
                    packet.CopyTo(copy);
                    net.SendQueue.Enqueue(copy);
                }
            }
            else if (Type == EntityType.Player)
            {
                var packet = Co2Packet.Serialize(ref msg);
                if (packet.Length == 0)
                    Debugger.Break();
                ref var net = ref Get<NetworkComponent>();
                var copy = new byte[packet.Length];
                packet.CopyTo(copy);
                net.SendQueue.Enqueue(copy);
            }
        }

        public override int GetHashCode() => Id;
        public override bool Equals(object? obj) => obj is NTT nttId && nttId.Id == Id;
        public static bool operator ==(in NTT a, in NTT b) => a.Id == b.Id;
        public static bool operator !=(in NTT a, in NTT b) => a.Id != b.Id;
        public override string ToString() => $"NTT {Id} ({Type})";
    }
}