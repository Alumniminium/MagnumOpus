using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class JumpSystem : PixelSystem<PositionComponent, JumpComponent, DirectionComponent>
    {
        public JumpSystem() : base("Jump System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && ntt.Type != EntityType.Npc && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref JumpComponent jmp, ref DirectionComponent dir)
        {
            var dist = (int)Vector2.Distance(pos.Position, jmp.Position);

            if(jmp.ChangedTick == PixelWorld.Tick)
            {
                var direction = CoMath.GetDirection(new Vector2(pos.Position.X, pos.Position.Y),new Vector2(jmp.Position.X, jmp.Position.Y));
                dir.Direction = direction;

                jmp.Time = CoMath.GetJumpTime(dist);
                // Console.WriteLine($"Jumping to {jmp.Position} | Dist: {dist} | Time: {jmp.Time:0.00}");
            }
            pos.Position = Vector2.Lerp(pos.Position, jmp.Position, jmp.Time);
            // Console.WriteLine($"Time: {jmp.Time:0.00}");
            jmp.Time -= deltaTime;

            if(jmp.Time <= 0)
            {
                pos.Position = jmp.Position;
                ntt.Remove<JumpComponent>();
                // ntt.NetSync(MsgAction.Create(0, ntt.Id, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, dir, MsgActionType.Teleport));
            }
        }
    }
}