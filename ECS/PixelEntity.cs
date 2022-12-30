using MagnumOpus.Components;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public readonly struct PixelEntity
    {
        public readonly int Id;
        public readonly int NetId;
        public readonly EntityType Type;
        public readonly int ParentId;
        public readonly PixelEntity[] Children;

        public PixelEntity(int id, int netId, EntityType type, int parentId = -1)
        {
            Id = id;
            NetId = netId;
            Type = type;
            ParentId = parentId;
            Children = Array.Empty<PixelEntity>();
        }

        public readonly void AttachChild(in PixelEntity child)
        {
            var children = Children;
            Array.Resize(ref children, children.Length + 1);
            children[^1] = child;
        }
        public readonly void Add<T>(ref T component) where T : struct => ComponentList<T>.AddFor(in this, ref component);
        public readonly ref T Get<T>() where T : struct => ref ComponentList<T>.Get(this);
        public readonly bool Has<T>() where T : struct => ComponentList<T>.HasFor(in this);
        public readonly bool Has<T, T2>() where T : struct where T2 : struct => Has<T>() && Has<T2>();
        public readonly bool Has<T, T2, T3>() where T : struct where T2 : struct where T3 : struct => Has<T, T2>() && Has<T3>();
        public readonly bool Has<T, T2, T3, T4>() where T : struct where T2 : struct where T3 : struct where T4 : struct => Has<T, T2, T3>() && Has<T4>();
        public readonly bool Has<T, T2, T3, T4, T5>() where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct => Has<T, T2, T3, T4>() && Has<T5>();
        public readonly void Remove<T>() => ReflectionHelper.Remove<T>(in this);
        public readonly void Recycle() => ReflectionHelper.RecycleComponents(in this);
        public readonly void NetSync<T>(ref T packet, bool broadcast = false) where T : unmanaged
        {
            var serialized = Co2Packet.Serialize(ref packet);
            NetSync(serialized, broadcast);
        }
        public readonly void NetSync(Memory<byte> packet, bool broadcast = false)
        {
            if(Type == EntityType.Player)
                OutgoingPacketQueue.Add(in this, in packet);
    
            if (broadcast)
            {
                ref readonly var vwp = ref Get<ViewportComponent>();
                for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
                {
                    var b = vwp.EntitiesVisible[i];
                    if (b.Type != EntityType.Player || b.Id == Id)
                        continue;

                    OutgoingPacketQueue.Add(in b, in packet);
                }
                 for (var i = 0; i < vwp.EntitiesVisibleLast.Count; i++)
                {
                    var b = vwp.EntitiesVisibleLast[i];
                    if (b.Type != EntityType.Player || b.Id == Id || vwp.EntitiesVisible.Contains(b))
                        continue;

                    OutgoingPacketQueue.Add(in b, in packet);
                }
            }
        }

        public override int GetHashCode() => Id;
        public override bool Equals(object? obj) => obj is PixelEntity nttId && nttId.Id == Id;
        public static bool operator ==(in PixelEntity a, in PixelEntity b) => a.Id == b.Id;
        public static bool operator !=(in PixelEntity a, in PixelEntity b) => a.Id != b.Id;
        public override string ToString() => $"NTT {Id} ({Type})";
    }
}