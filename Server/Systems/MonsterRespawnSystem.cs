using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Manages monster spawner entities that periodically create new monsters based on timer and population limits.
    /// Handles spawner validation, monster creation, and AI activation when players are visible.
    /// </summary>
    public sealed class MonsterRespawnSystem : NttSystem<SpawnerComponent, PositionComponent>
    {
        /// <summary>
        /// Initializes the MonsterRespawnSystem with limited threading for spawner processing.
        /// </summary>
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 2) { }

        /// <summary>
        /// Processes monster spawners, creating new monsters when timers expire and population limits allow.
        /// </summary>
        /// <param name="spawnerEntity">The spawner entity</param>
        /// <param name="spawnerComponent">Spawner component containing timing and monster data</param>
        /// <param name="position">Position component for spawn location</param>
        public override void Update(in NTT spawnerEntity, ref SpawnerComponent spawnerComponent, ref PositionComponent position)
        {
            if (spawnerComponent.RunTick > NttWorld.Tick)
                return;

            spawnerComponent.RunTick += NttWorld.TargetTps * spawnerComponent.TimerSeconds;

            if (spawnerComponent.Count >= spawnerComponent.MaxCount)
                return;

            if (!Collections.CqMonsterType.TryGetValue(spawnerComponent.MonsterId, out var monsterTypeData))
            {
                spawnerEntity.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine($"CQ_GENERATOR NPC TYPE {spawnerComponent.MonsterId} invalid!");
                return;
            }

            if (!Collections.Maps.TryGetValue(position.Map, out var mapData))
            {
                spawnerEntity.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine($"CQ_GENERATOR ID {spawnerComponent.GeneratorId} invalid map {position.Map}");
                return;
            }

            if (IsLogging)
                FConsole.WriteLine("{ntt} respawning {num} of {mob} on map {map}", spawnerEntity, spawnerComponent.GenPerTimer, monsterTypeData.name, mapData);


            for (var i = 0; i < spawnerComponent.GenPerTimer; i++)
            {
                var newMonster = EntityFactory.MakeMonster(monsterTypeData, ref spawnerComponent, position, spawnerEntity);

                ref var viewport = ref newMonster.Get<ViewportComponent>();
                Collections.SpatialHashes[position.Map].GetVisibleEntities(ref viewport);
                var playerVisible = false;

                foreach (var visibleEntity in viewport.EntitiesVisible)
                    visibleEntity.Set<ViewportUpdateTagComponent>();

                if (playerVisible)
                {
                    ref var brainComponent = ref newMonster.Get<BrainComponent>();
                    brainComponent.State = BrainState.WakingUp;
                }

                if (IsLogging)
                {
                    FConsole.WriteLine("{ntt} spawned {mob} at {pos}", newMonster, monsterTypeData.name, position.Position);
                    var spawnMessage = MsgText.Create(in spawnerEntity, "Respawning " + monsterTypeData.name + " at " + position.Position.X + ", " + position.Position.Y);
                    spawnerEntity.NetSync(ref spawnMessage, true);
                }
                if (spawnerComponent.Count >= spawnerComponent.MaxCount)
                    break;
            }
        }
    }
}