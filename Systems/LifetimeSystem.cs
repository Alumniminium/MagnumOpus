using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
    {
        private static readonly uint[] countdown = new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)NttWorld.TargetTps * sec).ToArray();
        public LifetimeSystem() : base("Lifetime", threads: 2) { IsLogging = false; }

        public override void Update(in NTT ntt, ref LifeTimeComponent ltc)
        {
            if (ltc.ExpireTick > NttWorld.Tick)
            {
                var ticksLeft = ltc.ExpireTick - NttWorld.Tick;

                if (!Array.Exists(countdown, x => x == ticksLeft))
                    return;

                if (IsLogging)
                    Logger.Debug("{ntt} -> {seconds} seconds left", ntt, ticksLeft / NttWorld.TargetTps);

                ref var pos = ref ntt.Get<PositionComponent>();
                pos.ChangedTick = NttWorld.Tick;

                var eff = MsgName.Create((ushort)pos.Position.X, (ushort)pos.Position.Y, "downnumber" + (ticksLeft / NttWorld.TargetTps), MsgNameType.MapEffect);
                ntt.NetSync(ref eff, true);
                return;
            }

            var dtc = new DeathTagComponent();
            ntt.Set(ref dtc);
            ntt.Remove<LifeTimeComponent>();
            if (IsLogging)
                Logger.Debug("{ntt} -> EXPIRED", ntt);
        }
    }
}