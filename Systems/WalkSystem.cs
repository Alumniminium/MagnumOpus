using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class WalkSystem : NttSystem<PositionComponent, WalkComponent, ViewportComponent>
    {
        public WalkSystem() : base("Walk", threads: Environment.ProcessorCount, log: false) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref WalkComponent wlk, ref ViewportComponent vwp)
        {
            var checkPos = pos.Position + Constants.DeltaPos[(int)wlk.Direction];

            foreach (var otherNtt in vwp.EntitiesVisible)
            {
                if (otherNtt == ntt)
                    continue;

                var otherPos = otherNtt.Get<PositionComponent>();

                if (otherPos.Position == checkPos && !otherNtt.Has<DeathTagComponent>())
                {
                    ntt.Remove<WalkComponent>();
                    return;
                }
            }

            pos.Direction = wlk.Direction;
            pos.LastPosition = pos.Position;
            pos.Position = checkPos;

            var pkt = MsgWalk.Create(ntt.Id, (byte)wlk.Direction, wlk.IsRunning);
            ntt.NetSync(ref pkt, true);

            if (IsLogging)
            {
                var text = $"Map: {pos.Map} -> {wlk.Direction} -> {pos.Position}";
                NetworkHelper.SendMsgTo(in ntt, text, MsgTextType.TopLeft);
                Logger.Debug("{ntt} walking {text}", ntt, text);
            }
            PrometheusPush.WalkCount.Inc();

            ntt.Remove<WalkComponent>();

            var shc = new SpatialHashUpdateComponent(pos.Position, pos.LastPosition, pos.Map, pos.Map, SpacialHashUpdatType.Move);
            ntt.Set(ref shc);

            ntt.Set<ViewportUpdateTagComponent>();
        }
    }
}