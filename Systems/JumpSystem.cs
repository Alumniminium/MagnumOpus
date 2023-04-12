using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class JumpSystem : NttSystem<PositionComponent, JumpComponent>
    {
        public JumpSystem() : base("Jump", threads: 2) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        public override void Update(in NTT ntt, ref PositionComponent pos, ref JumpComponent jmp)
        {
            var direction = CoMath.GetDirection(new Vector2(jmp.Position.X, jmp.Position.Y), pos.Position);
            var distance = (int)Vector2.Distance(new Vector2(jmp.Position.X, jmp.Position.Y), pos.Position);
            var jumpTime = NttWorld.TargetTps * CoMath.GetJumpTime(distance);

            pos.LastPosition = pos.Position;

            if (jmp.CreatedTick + jumpTime < NttWorld.Tick)
            {
                pos.Position = jmp.Position;
                ntt.Remove<JumpComponent>();
                if (IsLogging)
                    Logger.Debug("Jump complete for {ntt}", ntt);
            }
            else
                pos.Position = Vector2.Lerp(pos.Position, jmp.Position, 2 / jumpTime);

            if (jmp.CreatedTick == NttWorld.Tick)
            {
                pos.Direction = direction;
                var packet = MsgAction.CreateJump(in ntt, in jmp);
                ntt.NetSync(ref packet, true);

                // PrometheusPush.JumpCount.Inc();
                // PrometheusPush.JumpDistance.Inc(distance);
                if (IsLogging)
                    Logger.Debug("Jump started for {ntt}", ntt);
            }

            var shc = new SpatialHashUpdateComponent(pos.Position, pos.LastPosition, pos.Map, SpacialHashUpdatType.Move);
            ntt.Set(ref shc);
            ntt.Set<ViewportUpdateTagComponent>();
        }
    }
}