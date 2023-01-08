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

            var deltaX = Constants.DeltaX[(int)wlk.Direction];
            var deltaY = Constants.DeltaY[(int)wlk.Direction];
            pos.Position.X += deltaX;
            pos.Position.Y += deltaY;
            
            var text = $"{wlk.Direction} -> {pos.Position}";
            var msgText = MsgText.Create(in ntt, text, MsgTextType.Talk);
            var serialized = Co2Packet.Serialize(ref msgText, msgText.Size);
            ntt.NetSync(serialized);
            FConsole.WriteLine($"[{nameof(WalkSystem)}] {ntt.Id} -> {text}");
        }
    }
}