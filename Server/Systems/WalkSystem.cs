using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles entity movement and walking mechanics in the game world.
    /// Processes walk commands by updating position, direction, and notifying other systems.
    /// </summary>
    /// <example>
    /// // System automatically processes entities with PositionComponent, WalkComponent, and ViewportComponent
    /// var walkComponent = new WalkComponent(Direction.North, isRunning: true);
    /// entity.Set(ref walkComponent); // Triggers WalkSystem.Update() on next tick
    /// </example>
    public sealed class WalkSystem : NttSystem<PositionComponent, WalkComponent, ViewportComponent>
    {
        /// <summary>
        /// Initializes the WalkSystem with multi-threaded processing capabilities.
        /// </summary>
        public WalkSystem() : base("Walk", threads: Environment.ProcessorCount, log: false) { }

        /// <summary>
        /// Processes a single entity's walk request, updating position and notifying relevant systems.
        /// </summary>
        /// <param name="ntt">The entity requesting to walk</param>
        /// <param name="pos">Entity's position component (current location, direction, map)</param>
        /// <param name="wlk">Walk component containing movement direction and running state</param>
        /// <param name="vwp">Viewport component for tracking visible entities</param>
        /// <example>
        /// // Called automatically by ECS when entity has WalkComponent
        /// // Flow: Player input -> WalkComponent added -> Update() called -> Component removed
        /// </example>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref WalkComponent wlk, ref ViewportComponent vwp)
        {
            PrometheusPush.WalkCount.Inc();

            var newPosition = pos.Position + Constants.DeltaPos[(int)wlk.Direction];
            
            pos.Direction = wlk.Direction;
            pos.LastPosition = pos.Position;
            pos.Position = newPosition;

            var walkPacket = MsgWalk.Create(ntt.Id, (byte)wlk.Direction, wlk.IsRunning);
            ntt.NetSync(ref walkPacket, true);

            if (IsLogging && ntt.Has<NetworkComponent>())
            {
                var debugText = $"Map: {pos.Map} -> {wlk.Direction} -> {pos.Position}";
                NetworkHelper.SendMsgTo(in ntt, debugText, MsgTextType.TopLeft);
                FConsole.WriteLine("{ntt} walking {debugText}", ntt, debugText);
            }

            ref var emote = ref ntt.Get<EmoteComponent>();
            if (emote.Emote != Emote.Stand)
            {
                emote.Emote = Emote.Stand;
            }

            var spatialUpdate = new SpatialHashUpdateComponent(
                pos.Position,
                pos.LastPosition,
                pos.Map,
                pos.Map,
                SpacialHashUpdatType.Move
            );
            ntt.Set(ref spatialUpdate);

            ntt.Set<ViewportUpdateTagComponent>();
            ntt.Remove<WalkComponent>();
        }
    }
}