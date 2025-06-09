using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
{
    private static readonly uint[] countdown = [.. new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)NttWorld.TargetTps * sec)];

    public LifetimeSystem() : base("Lifetime", threads: 1, log: false) { IsLogging = false; }

    // Manages entities with limited lifetimes, showing countdown effects and marking expired
    // entities for death. Displays visual countdown numbers at 5, 4, 3, 2, 1 seconds remaining
    // before expiration using map effects. When lifetime expires, marks entity for death.
    public override void Update(in NTT ntt, ref LifeTimeComponent lifetimeComponent)
    {
        if (lifetimeComponent.ExpireTick > NttWorld.Tick)
        {
            // === SHOW COUNTDOWN EFFECTS ===
            var ticksLeft = lifetimeComponent.ExpireTick - NttWorld.Tick;

            // Only show countdown at specific intervals (5, 4, 3, 2, 1 seconds)
            if (!Array.Exists(countdown, x => x == ticksLeft))
                return;

            if (IsLogging)
                FConsole.WriteLine("{ntt} -> {seconds} seconds left", ntt, ticksLeft / NttWorld.TargetTps);

            // Display countdown number effect at entity position
            ref readonly var position = ref ntt.Get<PositionComponent>();
            var countdownEffect = MsgName.Create((ushort)position.Position.X, (ushort)position.Position.Y, "downnumber" + (ticksLeft / NttWorld.TargetTps), MsgNameType.MapEffect);
            ntt.NetSync(ref countdownEffect, broadcast: true);
            return;
        }

        // === HANDLE EXPIRATION ===
        // Mark entity for death when lifetime expires
        var deathTag = new DeathTagComponent();
        ntt.Set(ref deathTag);
        ntt.Remove<LifeTimeComponent>();

        if (IsLogging)
            FConsole.WriteLine("{ntt} -> EXPIRED", ntt);
    }
}