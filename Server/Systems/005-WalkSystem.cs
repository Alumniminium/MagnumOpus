using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class WalkSystem : NttSystem<PositionComponent, WalkComponent, ViewportComponent>
{
    public WalkSystem() : base("Walk", threads: 1, log: false) { }

    // Handles entity movement and walking mechanics in the game world. Processes walk commands
    // by updating position based on direction, broadcasting movement to nearby players, resetting
    // emotes to standing, updating spatial hash for collision detection, and triggering viewport
    // updates. Includes Prometheus metrics for movement tracking and debug output for networked entities.
    public override void Update(in NTT ntt, ref PositionComponent pos, ref WalkComponent wlk, ref ViewportComponent vwp)
    {
        // === TRACK MOVEMENT METRICS ===
        PrometheusPush.WalkCount.Inc();

        // === UPDATE POSITION ===
        // Calculate new position based on movement direction
        var newPosition = pos.Position + Constants.DeltaPos[(int)wlk.Direction];

        pos.Direction = wlk.Direction;
        pos.LastPosition = pos.Position;
        pos.Position = newPosition;

        // === BROADCAST MOVEMENT ===
        // Send walk packet to nearby players for visual synchronization
        var walkPacket = MsgWalk.Create(ntt.Id, (byte)wlk.Direction, wlk.IsRunning);
        ntt.NetSync(ref walkPacket, broadcast: true);

        // === DEBUG LOGGING FOR NETWORKED ENTITIES ===
        if (IsLogging)
        {
            var debugText = $"Map: {pos.Map} -> {wlk.Direction} -> {pos.Position}";
            FConsole.WriteLine("{ntt} walking {info}", ntt, debugText);
        }

        // === RESET EMOTE STATE ===
        // Walking automatically resets entities to standing emote
        ref var emote = ref ntt.Get<EmoteComponent>();
        if (emote.Emote != Emote.Stand)
            emote.Emote = Emote.Stand;

        // === UPDATE SPATIAL SYSTEMS ===
        // Update spatial hash for collision detection and visibility
        var spatialUpdate = new SpatialHashUpdateComponent(
            pos.Position,
            pos.LastPosition,
            pos.Map,
            pos.Map,
            SpacialHashUpdatType.Move
        );
        ntt.Set(ref spatialUpdate);

        // Trigger viewport updates for this entity
        ntt.Set<ViewportUpdateTagComponent>();

        // Clean up the walk component (one-time use)
        ntt.Remove<WalkComponent>();
    }
}