using MagnumOpus.Components.Death;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy", threads: 2, log: true) { }

        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
        {
            if (ntt.Has<PositionComponent>())
            {
                ref var pos = ref ntt.Get<PositionComponent>();
                if (Collections.SpatialHashs.TryGetValue(pos.Map, out var hash))
                    hash.Remove(ntt, ref pos);
            }

            switch (ntt.Type)
            {
                case EntityType.Player:
                case EntityType.Monster:
                    var despawn = MsgAction.RemoveEntity(ntt.Id);
                    ntt.NetSync(ref despawn, true);
                    break;
                case EntityType.Item:
                    var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                    ntt.NetSync(ref delete, true);
                    break;
            }

            NttWorld.Destroy(ntt);

            if (IsLogging)
                Logger.Debug("Destroyed {ntt}", ntt);
        }
    }
}