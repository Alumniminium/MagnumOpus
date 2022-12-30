using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class JumpSystem : PixelSystem<PositionComponent, JumpComponent, DirectionComponent>
    {
        public JumpSystem() : base("Jump System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref JumpComponent jmp, ref DirectionComponent dir)
        {
            pos.ChangedTick = PixelWorld.Tick;
            dir.ChangedTick = PixelWorld.Tick;

            var direction = CoMath.GetDirection(new Vector2(jmp.Position.X, jmp.Position.Y),pos.Position);
            dir.Direction = direction;

            pos.Position = jmp.Position;
            Game.Grids[pos.Map].Move(in ntt, ref pos);
            
            var text = $"{direction} -> {pos.Position}";
            var msgText = MsgText.Create(in ntt, text, MsgTextType.Talk);
            var serialized = Co2Packet.Serialize(ref msgText, msgText.Size);
            ntt.NetSync(serialized);
        }
    }
}