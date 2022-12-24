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
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref JumpComponent jmp, ref DirectionComponent dir)
        {
            pos.ChangedTick = PixelWorld.Tick;
            var direction = CoMath.GetDirection(pos.Position, new Vector2(jmp.Position.X, jmp.Position.Y));
            dir.Direction = direction;

            pos.Position = jmp.Position;
            Game.Grids[pos.Map].Move(in ntt, ref pos);

            var msg = MsgAction.CreateJump(in ntt, in jmp);
            ntt.NetSync(ref msg, true);
            ntt.Remove<JumpComponent>();
        }
    }
}