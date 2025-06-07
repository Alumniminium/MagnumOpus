using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Manages entities with limited lifetimes, showing countdown effects and marking expired entities for death.
    /// Displays visual countdown numbers at 5, 4, 3, 2, 1 seconds remaining before expiration.
    /// </summary>
    public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
    {
        private static readonly uint[] countdown = new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)NttWorld.TargetTps * sec).ToArray();

        /// <summary>
        /// Initializes the LifetimeSystem with limited threading for expiration processing.
        /// </summary>
        public LifetimeSystem() : base("Lifetime", threads: 1) { IsLogging = false; }

        /// <summary>
        /// Processes entities with limited lifetimes, showing countdown effects and marking expired entities for death.
        /// </summary>
        /// <param name="ntt">The entity with a lifetime component</param>
        /// <param name="lifetimeComponent">Lifetime component containing expiration information</param>
        public override void Update(in NTT ntt, ref LifeTimeComponent lifetimeComponent)
        {
            if (lifetimeComponent.ExpireTick > NttWorld.Tick)
            {
                var ticksLeft = lifetimeComponent.ExpireTick - NttWorld.Tick;

                if (!Array.Exists(countdown, x => x == ticksLeft))
                    return;

                if (IsLogging)
                    FConsole.WriteLine("{ntt} -> {seconds} seconds left", ntt, ticksLeft / NttWorld.TargetTps);

                ref readonly var position = ref ntt.Get<PositionComponent>();
                var countdownEffect = MsgName.Create((ushort)position.Position.X, (ushort)position.Position.Y, "downnumber" + (ticksLeft / NttWorld.TargetTps), MsgNameType.MapEffect);
                ntt.NetSync(ref countdownEffect, true);
                return;
            }

            var deathTag = new DeathTagComponent();
            ntt.Set(ref deathTag);
            ntt.Remove<LifeTimeComponent>();
            if (IsLogging)
                FConsole.WriteLine("{ntt} -> EXPIRED", ntt);
        }
    }
}