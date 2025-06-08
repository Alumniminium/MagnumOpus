using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class MonsterRespawnSystem : NttSystem<SpawnerComponent, PositionComponent>
    {
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 1, log: false) { }

        // Manages monster spawner entities that periodically create new monsters based on timer
        // and population limits. Validates spawner data and map existence, creates monsters using
        // EntityFactory, updates spatial hashes for visibility, and activates AI when players are
        // nearby. Spawners respect max count limits and timing intervals for balanced gameplay.
        public override void Update(in NTT spawner, ref SpawnerComponent spawnerComponent, ref PositionComponent pos)
        {
            // === CHECK SPAWN TIMING ===
            // Only process spawner when timer has elapsed
            if (spawnerComponent.RunTick > NttWorld.Tick)
                return;

            // Reset timer for next spawn cycle
            spawnerComponent.RunTick += NttWorld.TargetTps * spawnerComponent.TimerSeconds;

            // === VALIDATE POPULATION LIMITS ===
            // Don't spawn if at maximum capacity
            if (spawnerComponent.Count >= spawnerComponent.MaxCount)
                return;

            // === VALIDATE SPAWNER DATA ===
            // Check if monster type exists in database
            if (!Collections.CqMonsterType.TryGetValue(spawnerComponent.MonsterId, out var monsterTypeData))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine("CQ_GENERATOR NPC TYPE {id} invalid!", spawnerComponent.MonsterId);
                return;
            }

            // Check if map exists and is valid
            if (!Collections.Maps.TryGetValue(pos.Map, out var mapData))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine("CQ_GENERATOR ID {id} invalid map {map}", 
                    spawnerComponent.GeneratorId, pos.Map);
                return;
            }

            if (IsLogging)
                FConsole.WriteLine("{ntt} spawning {count} {monster} on map {map}", 
                    spawner, spawnerComponent.GenPerTimer, monsterTypeData.name, mapData);

            // === SPAWN MONSTERS ===
            // Create monsters up to the per-timer limit
            for (var i = 0; i < spawnerComponent.GenPerTimer; i++)
            {
                // create a new position for the monster
                pos.Position = CoMath.GetRandomPointInRect(in spawnerComponent.SpawnArea);

                var newMonster = EntityFactory.MakeMonster(monsterTypeData, ref spawnerComponent, pos, spawner);

                // Update spatial visibility for the new monster
                ref var viewport = ref newMonster.Get<ViewportComponent>();
                Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref viewport);

                // Mark all visible entities for viewport updates
                foreach (var visibleEntity in viewport.EntitiesVisible)
                    visibleEntity.Set<ViewportUpdateTagComponent>();

                // Note: playerVisible logic appears incomplete - currently always false
                // TODO: Actually check if players are visible to activate AI
                var playerVisible = false;
                if (playerVisible)
                {
                    ref var brainComponent = ref newMonster.Get<BrainComponent>();
                    brainComponent.State = BrainState.WakingUp;
                }

                if (IsLogging)
                {
                    FConsole.WriteLine("{monster} spawned at {pos}", newMonster, pos.Position);
                    var spawnMessage = MsgText.Create(in spawner, 
                        $"Respawning {monsterTypeData.name} at {pos.Position.X}, {pos.Position.Y}");
                    spawner.NetSync(ref spawnMessage, true);
                }

                // Stop spawning if we've reached maximum capacity
                if (spawnerComponent.Count >= spawnerComponent.MaxCount)
                    break;
            }
        }
    }
}