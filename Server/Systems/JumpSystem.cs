using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles entity jumping mechanics including position updates and spatial hash management.
    /// Processes jump requests by calculating direction, updating position, and broadcasting to nearby entities.
    /// </summary>
    public sealed class JumpSystem : NttSystem<PositionComponent, JumpComponent>
    {
        /// <summary>
        /// Initializes the JumpSystem with limited threading for jump processing.
        /// </summary>
        public JumpSystem() : base("Jump", threads: 2) { }
        
        /// <summary>
        /// Filters out item entities from jump processing since they cannot jump.
        /// </summary>
        /// <param name="ntt">Entity to check for jump processing eligibility</param>
        /// <returns>True if entity can jump (not an item)</returns>
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type != EntityType.Item && base.MatchesFilter(in ntt);

        /// <summary>
        /// Processes an entity's jump request, updating position and notifying relevant systems.
        /// </summary>
        /// <param name="ntt">The entity performing the jump</param>
        /// <param name="pos">Entity's position component for location updates</param>
        /// <param name="jmp">Jump component containing target destination</param>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref JumpComponent jmp)
        {
            var targetPosition = new Vector2(jmp.Position.X, jmp.Position.Y);
            var direction = CoMath.GetDirection(targetPosition, pos.Position);
            var distance = (int)Vector2.Distance(targetPosition, pos.Position);

            pos.LastPosition = pos.Position;
            pos.Position = jmp.Position;
            pos.Direction = direction;
            var jumpPacket = MsgAction.CreateJump(in ntt, in jmp);
            ntt.NetSync(ref jumpPacket, true);

            PrometheusPush.JumpCount.Inc();
            PrometheusPush.JumpDistance.Inc(distance);
            if (IsLogging)
                FConsole.WriteLine("Jump started for {ntt}", ntt);

            var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, pos.LastPosition, pos.Map, pos.Map, SpacialHashUpdatType.Move);
            ntt.Set(ref spatialUpdate);
            ntt.Set<ViewportUpdateTagComponent>();
            ntt.Remove<JumpComponent>();
        }
    }
}