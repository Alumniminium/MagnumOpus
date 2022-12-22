using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type == EntityType.Player && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref NetSyncComponent c1)
        {
            SelfUpdate(in ntt);

            if (ntt.Type != EntityType.Player)
                return;

            ref readonly var vwp = ref ntt.Get<ViewportComponent>();

            for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                var changedEntity = vwp.EntitiesVisible[x];
                Update(in ntt, in changedEntity);
            }
        }

        public static void SelfUpdate(in PixelEntity ntt) => Update(in ntt, in ntt);

        public static void Update(in PixelEntity ntt, in PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();
            ref readonly var net = ref ntt.Get<NetSyncComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var dir = ref ntt.Get<DirectionComponent>();
            
            if (syn.Fields.HasFlag(SyncThings.Health) || syn.Fields.HasFlag(SyncThings.MaxHealth))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
                if(hlt.ChangedTick == PixelWorld.Tick)
                {
                    var healthMsg = MsgUserAttrib.Create(ntt.NetId, hlt.Health, MsgUserAttribType.Health);
                    var maxHealthMsg = MsgUserAttrib.Create(ntt.NetId, hlt.MaxHealth, MsgUserAttribType.MaxHealth);
                    ntt.NetSync(in healthMsg);
                    ntt.NetSync(in maxHealthMsg);
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Level))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if(lvl.ChangedTick == PixelWorld.Tick)
                {
                    var lvlMsg = MsgUserAttrib.Create(ntt.NetId, lvl.Level, MsgUserAttribType.Level);
                    ntt.NetSync(in lvlMsg);
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Experience))
            {
                ref readonly var exp = ref other.Get<ExperienceComponent>();
                if(exp.ChangedTick == PixelWorld.Tick)
                {
                    var expMsg = MsgUserAttrib.Create(ntt.NetId, exp.Experience, MsgUserAttribType.Experience);
                    ntt.NetSync(in expMsg);
                }
            }
        }
    }
}