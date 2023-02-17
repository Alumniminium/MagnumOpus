using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
    {
        private static readonly uint[] countdown = new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)NttWorld.TargetTps * sec).ToArray();
        public LifetimeSystem() : base("Lifetime", threads: 4) { Trace = true; }

        public override void Update(in NTT ntt, ref LifeTimeComponent ltc)
        {
            if (ltc.ExpireTick > NttWorld.Tick)
            {
                var ticksLeft = ltc.ExpireTick - NttWorld.Tick;

                if (!Array.Exists(countdown, x => x == ticksLeft))
                    return;

                string text = $"{ticksLeft / NttWorld.TargetTps} seconds left";
                if (Trace)
                    FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> {text}");

                ref readonly var pos = ref ntt.Get<PositionComponent>();
                var eff = MsgName.Create((ushort)pos.Position.X, (ushort)pos.Position.Y, "downnumber" + (ticksLeft / NttWorld.TargetTps), MsgNameType.MapEffect);
                // var eff = MsgName.Create(ntt.NetId,"mass",(byte)MsgNameType.RoleEffect);
                ntt.NetSync(ref eff, true);
                return;
            }

            ref readonly var pos2 = ref ntt.Get<PositionComponent>();
            var effect = MsgName.Create((ushort)pos2.Position.X, (ushort)pos2.Position.Y, "ballblast", MsgNameType.RoleEffect);
            ntt.NetSync(ref effect, true);
            var dtc = new DeathTagComponent(ntt.Id, default);
            ntt.Set(ref dtc);
            ntt.Remove<LifeTimeComponent>();
            if (Trace)
                FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> EXPIRED");
        }
    }
}