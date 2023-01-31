using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class WalkSystem : PixelSystem<PositionComponent, WalkComponent, DirectionComponent>
    {
        public WalkSystem() : base("Walk System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref WalkComponent wlk, ref DirectionComponent dir)
        {
            dir.ChangedTick = PixelWorld.Tick;
            pos.ChangedTick = PixelWorld.Tick;

            dir.Direction = wlk.Direction;
            pos.Position += Constants.DeltaPos[(int)wlk.Direction];

            var pkt = MsgWalk.Create(ntt.NetId, (byte)wlk.Direction, wlk.IsRunning);
            ntt.NetSync(ref pkt, true);

            // var eff = MsgFloorItem.Create((int)PixelWorld.Tick, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.DisplayEffect);
            // var deff = MsgFloorItem.Create((int)PixelWorld.Tick-1, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.RemoveEffect);
            // ntt.NetSync(ref eff, true);
            // ntt.NetSync(ref deff, true);
            
            var text = $"{wlk.Direction} -> {pos.Position}";
            NetworkHelper.SendMsgTo(in ntt, text, MsgTextType.TopLeft);
            
            // FConsole.WriteLine($"[{nameof(WalkSystem)}] {ntt.Id} -> {text}");

            ntt.Remove<WalkComponent>();
        }
    }
}