using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity ntt)
        {
            return ntt.Type == EntityType.Player && base.MatchesFilter(ntt);
        }

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
            
            if (syn.Fields.HasFlag(SyncThings.Position))
            {
                ref var bdy = ref other.Get<BodyComponent>(); 
            }
            if (syn.Fields.HasFlag(SyncThings.Health))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
            }
            if (syn.Fields.HasFlag(SyncThings.Level))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
            }
            if (syn.Fields.HasFlag(SyncThings.Experience))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
            }
        }
    }
}