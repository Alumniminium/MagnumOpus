using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class MonsterRespawnSystem : NttSystem<SpawnerComponent, PositionComponent>
    {
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 2) { }

        public override void Update(in NTT spawner, ref SpawnerComponent spc, ref PositionComponent pos)
        {
            if (spc.RunTick > NttWorld.Tick)
                return;

            spc.RunTick += NttWorld.TargetTps * spc.TimerSeconds;

            if (spc.Count >= spc.MaxCount)
                return;

            if (!Collections.CqMonsterType.TryGetValue(spc.MonsterId, out var cqMob))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine($"CQ_GENERATOR NPC TYPE {spc.MonsterId} invalid!");
                return;
            }

            if (!Collections.Maps.TryGetValue(pos.Map, out var cqMap))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                FConsole.WriteLine($"CQ_GENERATOR ID {spc.GeneratorId} invalid map {pos.Map}");
                return;
            }

            if (IsLogging)
                Logger.Debug("{ntt} respawning {num} of {mob} on map {map}", spawner, spc.GenPerTimer, cqMob.name, cqMap);


            for (var i = 0; i < spc.GenPerTimer; i++)
            {
                var mob = EntityFactory.MakeMonster(cqMob, ref spc, pos, spawner);

                ref var vwp = ref mob.Get<ViewportComponent>();
                Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref vwp);
                var playerVisible = false;

                foreach (var visible in vwp.EntitiesVisible)
                    visible.Set<ViewportUpdateTagComponent>();

                if (playerVisible)
                {
                    ref var brain = ref mob.Get<BrainComponent>();
                    brain.State = BrainState.WakingUp;
                }

                if (IsLogging)
                {
                    Logger.Debug("{ntt} spawned {mob} at {pos}", mob, cqMob.name, pos.Position);
                    var msgTalk = MsgText.Create(in spawner, "Respawning " + cqMob.name + " at " + pos.Position.X + ", " + pos.Position.Y);
                    spawner.NetSync(ref msgTalk, true);
                }
                if (spc.Count >= spc.MaxCount)
                    break;
            }
        }
    }
}