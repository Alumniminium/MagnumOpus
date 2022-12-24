using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class JumpSystem : PixelSystem<PositionComponent, JumpComponent, DirectionComponent>
    {
        public JumpSystem() : base("Jump System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && ntt.Type != EntityType.Npc && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref JumpComponent jmp, ref DirectionComponent dir)
        {
            // if (jmp.ChangedTick == PixelWorld.Tick)
            // {
            // var dist = (int)Vector2.Distance(pos.Position, jmp.Position);
            var direction = CoMath.GetDirection(new Vector2(pos.Position.X, pos.Position.Y), new Vector2(jmp.Position.X, jmp.Position.Y));
            dir.Direction = direction;
            // 
            // jmp.Time = CoMath.GetJumpTime(dist);
            // Console.WriteLine($"Jumping to {jmp.Position} | Dist: {dist} | Time: {jmp.Time:0.00}");
            // }

            // pos.Position = Vector2.Lerp(pos.Position, jmp.Position, jmp.Time); 
            pos.ChangedTick = PixelWorld.Tick;
            // Console.WriteLine($"Time: {jmp.Time:0.00}");
            // jmp.Time -= deltaTime;

            // if (jmp.Time <= 0)
            // {
            pos.Position = jmp.Position;
            var msg = MsgAction.CreateJump(in ntt, in jmp);
            ntt.NetSync(ref msg, true);
            ntt.Remove<JumpComponent>();
            Game.Grids[pos.Map].Move(in ntt, ref pos);
            //     FConsole.WriteLine($"[{nameof(JumpSystem)}] {ntt.NetId} -> {dir.Direction} | {pos.Position}");
            // }
        }
    }
}