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

        public static void SelfUpdate(in PixelEntity ntt)
        {
            Update(in ntt, in ntt);
        }

        public static void Update(in PixelEntity ntt, in PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();
            
            if (syn.Fields.HasFlag(SyncThings.Position))
            {
                ref var phy = ref other.Get<BodyComponent>(); 

                if (Game.CurrentTick == phy.ChangedTick)
                    ntt.NetSync(MsgWalk.Create(other.Id, phy.Direction, false));
            }
            if (syn.Fields.HasFlag(SyncThings.Health))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
                if (Game.CurrentTick == hlt.ChangedTick)
                {
                    ntt.NetSync(MsgUpdate.Create(other.Id, hlt.Health, MsgUpdateType.Hp));
                    ntt.NetSync(MsgUpdate.Create(other.Id, hlt.MaxHealth, MsgUpdateType.MaxHp));
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Level))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if (Game.CurrentTick == lvl.ChangedTick)
                    ntt.NetSync(MsgUpdate.Create(other.Id, lvl.Level, MsgUpdateType.Level));
            }
            if (syn.Fields.HasFlag(SyncThings.Experience))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if (Game.CurrentTick == lvl.ChangedTick)
                    ntt.NetSync(MsgUpdate.Create(other.Id, lvl.Experience, MsgUpdateType.Exp));
            }
        }
    }
}