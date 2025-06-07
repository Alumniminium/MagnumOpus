using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles player revival after death, restoring health and teleporting to respawn locations.
    /// Manages death status cleanup, appearance restoration, and spawn location database lookups.
    /// </summary>
    public sealed class ReviveSystem : NttSystem<ReviveComponent, HealthComponent, PositionComponent, BodyComponent, StatusEffectComponent>
    {
        /// <summary>
        /// Initializes the ReviveSystem with limited threading for revival processing.
        /// </summary>
        public ReviveSystem() : base("Revive", threads: 1) { }

        /// <summary>
        /// Processes player revival, restoring health and teleporting to appropriate respawn location.
        /// </summary>
        /// <param name="ntt">The entity being revived</param>
        /// <param name="reviveComponent">Revive component containing timing information</param>
        /// <param name="healthComponent">Health component to restore</param>
        /// <param name="position">Position component for respawn location</param>
        /// <param name="bodyComponent">Body component for appearance restoration</param>
        /// <param name="statusEffects">Status effect component to clear death effects</param>
        public override void Update(in NTT ntt, ref ReviveComponent reviveComponent, ref HealthComponent healthComponent, ref PositionComponent position, ref BodyComponent bodyComponent, ref StatusEffectComponent statusEffects)
        {
            if (reviveComponent.ReviveTick < NttWorld.Tick)
                return;

            FConsole.WriteLine($"[{nameof(ReviveSystem)}]: Revive on Map {position.Map}");

            healthComponent.Health = healthComponent.MaxHealth;
            using var databaseContext = new SquigglyContext();
            var currentMap = databaseContext.cq_map.Find((long)position.Map);

            if (currentMap != null)
            {
                var currentMapId = position.Map;
                var respawnMap = databaseContext.cq_map.FirstOrDefault(x => x.id == currentMap.reborn_map);

                if (respawnMap != null)
                {
                    position.Position = new Vector2(respawnMap.portal0_x, respawnMap.portal0_y);  // Auto-tracked
                    position.Map = (ushort)respawnMap.id;
                }
                else
                {
                    if (IsLogging)
                        FConsole.WriteLine("Reborn Map {0} not found", currentMapId);
                    position.Map = 1002;
                    position.Position = new Vector2(477, 380);  // Auto-tracked
                }
            }
            else
            {
                if (IsLogging)
                    FConsole.WriteLine("Map {0} not found", position.Map);
                position.Map = 1002;
                position.Position = new Vector2(477, 380);  // Auto-tracked
            }

            healthComponent.Health = healthComponent.MaxHealth;
            statusEffects.Effects &= ~StatusEffect.Dead;
            statusEffects.Effects &= ~StatusEffect.FrozenRemoveName;

            bodyComponent.Look = MsgSpawn.DelTransform(bodyComponent.Look);
            var locationMessage = MsgAction.Create(ntt.Id, position.Map, (ushort)position.Position.X, (ushort)position.Position.Y, Direction.North, MsgActionType.SendLocation);
            NetworkHelper.Despawn(ntt);

            ntt.NetSync(ref locationMessage);

            ntt.Set(ref position);
            ntt.Remove<ReviveComponent>();
            ntt.Remove<DeathTagComponent>();
            ntt.Set<ViewportUpdateTagComponent>();

            if (IsLogging)
                FConsole.WriteLine("Revived '{0}' at {1}, {2}, {3}", NttWorld.Tick, Name, ntt, position.Map, position.Position.X, position.Position.Y);
        }
    }
}