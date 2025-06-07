using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class ReviveSystem : NttSystem<ReviveComponent, HealthComponent, PositionComponent, BodyComponent, StatusEffectComponent>
    {
        public ReviveSystem() : base("Revive", threads: 1, log: false) { }

        // Handles player revival after death, restoring health and teleporting to respawn locations.
        // Manages death status cleanup, appearance restoration, and spawn location database lookups.
        // Uses map database to find respawn points or defaults to Twin City if lookup fails.
        public override void Update(in NTT ntt, ref ReviveComponent reviveComponent, ref HealthComponent healthComponent, ref PositionComponent position, ref BodyComponent bodyComponent, ref StatusEffectComponent statusEffects)
        {
            if (reviveComponent.ReviveTick < NttWorld.Tick)
                return;

            if (IsLogging)
                FConsole.WriteLine("[{system}]: Revive on Map {map}", nameof(ReviveSystem), position.Map);

            // === RESTORE HEALTH ===
            healthComponent.Health = healthComponent.MaxHealth;

            // === DETERMINE RESPAWN LOCATION ===
            using var databaseContext = new SquigglyContext();
            var currentMap = databaseContext.cq_map.Find((long)position.Map);

            if (currentMap != null)
            {
                var currentMapId = position.Map;
                var respawnMap = databaseContext.cq_map.FirstOrDefault(x => x.id == currentMap.reborn_map);

                if (respawnMap != null)
                {
                    // Use configured respawn location
                    position.Position = new Vector2(respawnMap.portal0_x, respawnMap.portal0_y);
                    position.Map = (ushort)respawnMap.id;
                }
                else
                {
                    // Fallback to Twin City if respawn map not found
                    if (IsLogging)
                        FConsole.WriteLine("Reborn Map {map} not found", currentMapId);
                    position.Map = 1002;
                    position.Position = new Vector2(477, 380);
                }
            }
            else
            {
                // Fallback to Twin City if current map not found
                if (IsLogging)
                    FConsole.WriteLine("Map {map} not found", position.Map);
                position.Map = 1002;
                position.Position = new Vector2(477, 380);
            }

            // === CLEAR DEATH STATUS EFFECTS ===
            statusEffects.Effects &= ~StatusEffect.Dead;
            statusEffects.Effects &= ~StatusEffect.FrozenRemoveName;

            // === RESTORE APPEARANCE ===
            bodyComponent.Look = MsgSpawn.DelTransform(bodyComponent.Look);

            // === NETWORK SYNCHRONIZATION ===
            var locationMessage = MsgAction.Create(ntt.Id, position.Map, (ushort)position.Position.X, (ushort)position.Position.Y, Direction.North, MsgActionType.SendLocation);
            NetworkHelper.Despawn(ntt);
            ntt.NetSync(ref locationMessage);

            // === FINALIZE REVIVAL ===
            ntt.Set(ref position);
            ntt.Remove<ReviveComponent>();
            ntt.Remove<DeathTagComponent>();
            ntt.Set<ViewportUpdateTagComponent>();

            if (IsLogging)
                FConsole.WriteLine("Revived {player} at map {map}, pos ({x}, {y})", ntt, position.Map, position.Position.X, position.Position.Y);
        }
    }
}