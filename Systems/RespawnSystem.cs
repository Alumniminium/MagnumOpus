using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class MonsterRespawnSystem : NttSystem<RespawnTagComponent, SpawnComponent, HealthComponent>
    {
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 2) { }

        public override void Update(in NTT ntt, ref RespawnTagComponent rtc, ref SpawnComponent spc, ref HealthComponent hpc)
        {
            if (rtc.RespawnTimeTick > NttWorld.Tick)
                return;

            Logger.Debug("{ntt} respawning", ntt);

            var respawnPos = new Vector2(Random.Shared.Next(spc.Spawn.bound_x, spc.Spawn.bound_x + spc.Spawn.bound_cx), Random.Shared.Next(spc.Spawn.bound_y, spc.Spawn.bound_y + spc.Spawn.bound_cy));

            var pos = new PositionComponent(ntt.Id, respawnPos, spc.Spawn.mapid);
            ntt.Set(ref pos);

            Collections.SpatialHashs[pos.Map].Add(in ntt);

            var brain = new BrainComponent(ntt.Id)
            {
                State = Enums.BrainState.WakingUp
            };
            ntt.Set(ref brain);

            hpc.Health = hpc.MaxHealth;

            ntt.Remove<StatusEffectComponent>();

            var eff = new StatusEffectComponent(ntt.Id);
            ntt.Set(ref eff);

            ref var cqm = ref ntt.Get<CqMonsterComponent>(); 
            ref var inv = ref ntt.Get<InventoryComponent>();

            var items = ItemGenerator.GetDropItemsFor(cqm.CqMonsterId);
            for (int x = 0; x < items.Count; x++)
            {
                var item = items[x];
                bool found = false;

                for (int y = 0; y < inv.Items.Length; y++)
                {
                    if (inv.Items[y].Id == 0)
                        continue;

                    ref var invItem = ref inv.Items[y].Get<ItemComponent>();
                    if (invItem.Id == item.ID)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    continue;

                var idx = Array.IndexOf(inv.Items, default);
                if (idx == -1)
                    continue;

                var invItemNtt = EntityFactory.MakeDefaultItem(item.ID, default, 0, true);
                if (invItemNtt== null)
                    continue;
                
                inv.Items[idx] = invItemNtt.Value;
            }

            ntt.Remove<RespawnTagComponent>();
        }
    }
}