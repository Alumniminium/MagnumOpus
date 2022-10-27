using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;

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
            
            if (syn.Fields.HasFlag(SyncThings.Walk))
            {
                ref readonly var wlk = ref other.Get<WalkComponent>();
                if(wlk.ChangedTick == PixelWorld.Tick)
                {
                    var walkMsg = MsgWalk.Create(other.Id, wlk.Direction, wlk.IsRunning);
                    ntt.NetSync(in walkMsg);
                }
            }
            if(syn.Fields.HasFlag(SyncThings.Jump))
            {
                ref readonly var jmp = ref other.Get<JumpComponent>();
                if(jmp.CreatedTick == PixelWorld.Tick)
                {
                    var jumpMsg = MsgAction.Create(0, ntt.Id, pos.Map, (ushort)pos.Position.X, (ushort)pos.Position.Y, dir.Direction, MsgActionType.Jump);
                    ntt.NetSync(in jumpMsg);
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Health) || syn.Fields.HasFlag(SyncThings.MaxHealth))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
                if(hlt.ChangedTick == PixelWorld.Tick)
                {
                    var updates = new MsgUserAttribValues[2];
                    updates[0] = new (hlt.Health, MsgUserAttribType.Health);
                    updates[1] = new (hlt.MaxHealth, MsgUserAttribType.MaxHealth);

                    var healthMsg = MsgUserAttrib.Create(ntt.Id, in updates);
                    ntt.NetSync(in healthMsg);
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Level))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if(lvl.ChangedTick == PixelWorld.Tick)
                {
                    var lvlMsg = MsgUserAttrib.Create(ntt.Id, lvl.Level, MsgUserAttribType.Level);
                    ntt.NetSync(in lvlMsg);
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Experience))
            {
                ref readonly var exp = ref other.Get<ExperienceComponent>();
                if(exp.ChangedTick == PixelWorld.Tick)
                {
                    var expMsg = MsgUserAttrib.Create(ntt.Id, exp.Experience, MsgUserAttribType.Experience);
                    ntt.NetSync(in expMsg);
                }
            }
        }
    }
}