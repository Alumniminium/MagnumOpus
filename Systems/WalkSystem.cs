

using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class WalkSystem : NttSystem<PositionComponent, WalkComponent, BodyComponent>
    {
        public WalkSystem() : base("Walk", threads: 2) { IsLogging = false; }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref WalkComponent wlk, ref BodyComponent bdy)
        {
            bdy.ChangedTick = NttWorld.Tick;
            pos.ChangedTick = NttWorld.Tick;

            ref var vpc = ref ntt.Get<ViewportComponent>();
            vpc.Dirty = true;

            bdy.Direction = wlk.Direction;
            pos.Position += Constants.DeltaPos[(int)wlk.Direction];

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
        }
    }
}