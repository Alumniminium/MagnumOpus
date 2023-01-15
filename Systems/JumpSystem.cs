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

            var direction = CoMath.GetDirection(new Vector2(jmp.Position.X, jmp.Position.Y), pos.Position);
            var distance  = (int)Vector2.Distance(new Vector2(jmp.Position.X, jmp.Position.Y), pos.Position);
            var jumpTime  = PixelWorld.TargetTps * CoMath.GetJumpTime(distance);
            
            if (jmp.CreatedTick + jumpTime < PixelWorld.Tick)
            {
                pos.Position = jmp.Position;
                ntt.Remove<JumpComponent>();
            }
            else
            {
                pos.Position = Vector2.Lerp(pos.Position, jmp.Position, 2 / jumpTime);
            }
            // var eff = MsgFloorItem.Create((int)PixelWorld.Tick, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.DisplayEffect);
            // var deff = MsgFloorItem.Create((int)PixelWorld.Tick-1, (ushort)pos.Position.X, (ushort)pos.Position.Y, 12, MsgFloorItemType.RemoveEffect);
            // ntt.NetSync(ref eff, true);
            // ntt.NetSync(ref deff, true);

            // var text = $"{direction} -> {pos.Position}";
            // var msgText = MsgText.Create(in ntt, text, MsgTextType.Talk);

            if (jmp.CreatedTick == PixelWorld.Tick)
            {
                dir.Direction = direction;
                var packet = MsgAction.CreateJump(in ntt, in jmp);
                ntt.NetSync(ref packet, true);
            }
        }
    }
}