using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class JumpSystem : NttSystem<PositionComponent, JumpComponent>
    {
        public JumpSystem() : base("Jump", threads: 1, log: false) { }

        protected override bool MatchesFilter(in NTT ntt) => !ntt.IsItem() && base.MatchesFilter(in ntt);

        // Handles entity jumping mechanics including position updates and spatial hash management.
        // Processes jump requests by calculating direction and distance to target, updating entity
        // position instantly, broadcasting jump animation to nearby players, tracking metrics for
        // monitoring, and updating spatial systems for collision detection and visibility.
        public override void Update(in NTT ntt, ref PositionComponent pos, ref JumpComponent jmp)
        {
            // === CALCULATE JUMP PARAMETERS ===
            var targetPosition = new Vector2(jmp.Position.X, jmp.Position.Y);
            var direction = CoMath.GetDirection(targetPosition, pos.Position);
            var distance = (int)Vector2.Distance(targetPosition, pos.Position);

            // === UPDATE POSITION INSTANTLY ===
            // Jumps are instant teleportation, not gradual movement
            pos.LastPosition = pos.Position;
            pos.Position = jmp.Position;
            pos.Direction = direction;

            // === BROADCAST JUMP ANIMATION ===
            // Send jump packet to nearby players for visual effects
            var jumpPacket = MsgAction.CreateJump(in ntt, in jmp);
            ntt.NetSync(ref jumpPacket, broadcast: true);

            if (IsLogging)
                FConsole.WriteLine("{ntt} jumped {distance} units to {pos}", ntt, distance, jmp.Position);

            // === UPDATE SPATIAL SYSTEMS ===
            // Update spatial hash for new position
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

            // Clean up the jump component (one-time use)
            ntt.Remove<JumpComponent>();

            // === TRACK JUMP METRICS ===
            PrometheusPush.JumpCount.Inc();
            PrometheusPush.JumpDistance.Inc(distance);
        }
    }
}