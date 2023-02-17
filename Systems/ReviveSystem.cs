using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ReviveSystem : NttSystem<ReviveComponent, HealthComponent, PositionComponent, BodyComponent, StatusEffectComponent>
    {
        public ReviveSystem() : base("Revive", threads: 2) { }

        public override void Update(in NTT ntt, ref ReviveComponent rev, ref HealthComponent hlt, ref PositionComponent pos, ref BodyComponent bdy, ref StatusEffectComponent eff)
        {
            if (rev.ReviveTick < NttWorld.Tick)
                return;

            FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Revive on Map {pos.Map}");

            hlt.Health = hlt.MaxHealth;
            using var ctx = new SquigglyContext();
            var map = ctx.cq_map.Find((long)pos.Map);

            if (map != null)
            {
                var mapId = pos.Map;
                var rebornMap = ctx.cq_map.FirstOrDefault(x => x.id == map.reborn_map);

                if (rebornMap != null)
                {
                    pos.ChangedTick = NttWorld.Tick + 1;
                    pos.Position = new Vector2(rebornMap.portal0_x, rebornMap.portal0_y);
                    pos.Map = (ushort)rebornMap.id;
                    FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Revive at portal: {pos.Map}  x: {pos.Position.X} y: {pos.Position.Y}");
                }
                else
                {
                    FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Reborn Map {pos.Map} not found");
                    pos.Map = 1002;
                    pos.Position = new Vector2(477, 380);
                }
            }
            else
            {
                FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Map {pos.Map} not found");
                pos.Map = 1002;
                pos.Position = new Vector2(477, 380);
            }

            hlt.Health = hlt.MaxHealth;
            eff.Effects &= ~StatusEffect.Dead;
            eff.Effects &= ~StatusEffect.FrozenRemoveName;

            var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
            var hltUp = MsgUserAttrib.Create(ntt.NetId, (ulong)hlt.Health, MsgUserAttribType.Health);
            var bdyUp = MsgUserAttrib.Create(ntt.NetId, (ulong)bdy.Look, MsgUserAttribType.Look);
            var reply = MsgAction.Create(ntt.NetId, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);

            ntt.NetSync(ref hltUp);
            ntt.NetSync(ref bdyUp);
            ntt.NetSync(ref update);
            ntt.NetSync(ref reply);

            ntt.Set(ref pos);
            ntt.Remove<ReviveComponent>();
            ntt.Remove<DeathTagComponent>();

            FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Revived {ntt.NetId} at {pos.Map} at {pos.Position.X}, {pos.Position.Y}");
        }
    }
}