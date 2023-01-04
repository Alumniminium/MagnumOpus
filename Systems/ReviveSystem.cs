using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ReviveSystem : PixelSystem<ReviveComponent, HealthComponent, PositionComponent, BodyComponent, StatusEffectComponent>
    {
        public ReviveSystem() : base("Revive System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref ReviveComponent rev, ref HealthComponent hlt, ref PositionComponent pos, ref BodyComponent bdy, ref StatusEffectComponent eff)
        {
            if (rev.ReviveTick < PixelWorld.Tick)
                return;

            hlt.Health = hlt.MaxHealth;
            using var ctx = new SquigglyContext();
            var map = ctx.cq_map.Find((long)pos.Map);

            pos.ChangedTick = PixelWorld.Tick+1;

            if(map.reborn_map == pos.Map)
            {
                pos.Position = new Vector2(map.portal0_x, map.portal0_y);
            }
            else
            {
                var portalId = map.reborn_portal;
                var portal = ctx.cq_portal.Find((int)portalId);
                pos.Map = (ushort)map.reborn_map;
            }

            hlt.Health = hlt.MaxHealth;
            eff.Effects &= ~StatusEffect.Dead;

            var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
            var hltUp = MsgUserAttrib.Create(ntt.NetId, (ulong)hlt.Health, MsgUserAttribType.Health);
            var bdyUp = MsgUserAttrib.Create(ntt.NetId, (ulong)bdy.Look, MsgUserAttribType.Look);
            var reply = MsgAction.Create(ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);

            ntt.NetSync(ref hltUp);
            ntt.NetSync(ref bdyUp);
            ntt.NetSync(ref update);
            ntt.NetSync(ref reply);

            ntt.Add(ref pos);
            ntt.Remove<ReviveComponent>();
        }
    }
}