using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
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

                    pos.ChangedTick = NttWorld.Tick;
                    pos.Position = new Vector2(rebornMap.portal0_x, rebornMap.portal0_y);
                    pos.Map = (ushort)rebornMap.id;
                }
                else
                {
                    if (IsLogging)
                        Logger.Debug("Reborn Map {0} not found", mapId);
                    pos.Map = 1002;
                    pos.ChangedTick = NttWorld.Tick;
                    pos.Position = new Vector2(477, 380);
                }
            }
            else
            {
                if (IsLogging)
                    Logger.Debug("Map {0} not found", pos.Map);
                pos.Map = 1002;

                pos.ChangedTick = NttWorld.Tick;
                pos.Position = new Vector2(477, 380);
            }

            hlt.Health = hlt.MaxHealth;
            eff.Effects &= ~StatusEffect.Dead;
            eff.Effects &= ~StatusEffect.FrozenRemoveName;

            bdy.Look = MsgSpawn.DelTransform(bdy.Look);

            var reply = MsgAction.Create(ntt.Id, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, Direction.North, MsgActionType.SendLocation);

            ntt.NetSync(ref reply);

            ntt.Set(ref pos);
            ntt.Remove<ReviveComponent>();
            ntt.Remove<DeathTagComponent>();
            ntt.Set<ViewportUpdateTagComponent>();

            if (IsLogging)
                Logger.Debug("Revived '{0}' at {1}, {2}, {3}", NttWorld.Tick, Name, ntt, pos.Map, pos.Position.X, pos.Position.Y);
        }
    }
}