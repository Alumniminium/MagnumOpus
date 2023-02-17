using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class WalkSystem : NttSystem<PositionComponent, WalkComponent, BodyComponent>
    {
        public WalkSystem() : base("Walk", threads: 6) { Trace = true; }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref WalkComponent wlk, ref BodyComponent bdy)
        {
            bdy.ChangedTick = NttWorld.Tick;
            pos.ChangedTick = NttWorld.Tick;

            bdy.Direction = wlk.Direction;
            pos.Position += Constants.DeltaPos[(int)wlk.Direction];

            var pkt = MsgWalk.Create(ntt.NetId, (byte)wlk.Direction, wlk.IsRunning);
            ntt.NetSync(ref pkt, true);

            // var eff = MsgFloorItem.Create((int)PixelWorld.Tick, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.DisplayEffect);
            // ntt.NetSync(ref eff, true);
            // var deff = MsgFloorItem.Create((int)PixelWorld.Tick-1, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.RemoveEffect);
            // ntt.NetSync(ref deff, true);

            var text = $"{wlk.Direction} -> {pos.Position}";
            NetworkHelper.SendMsgTo(in ntt, text, MsgTextType.TopLeft);
            //FConsole.WriteLine($"[{NttWorld.Tick}][{nameof(WalkSystem)}] (Thread {Environment.CurrentManagedThreadId}) {ntt.Id} -> {text}");

            ntt.Remove<WalkComponent>();
        }
    }
}