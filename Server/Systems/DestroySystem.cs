using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles final cleanup and destruction of entities marked for removal.
    /// Sends appropriate despawn packets to clients before destroying entities from the world.
    /// </summary>
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        /// <summary>
        /// Initializes the DestroySystem with limited threading for cleanup operations.
        /// </summary>
        public DestroySystem() : base("Destroy", threads: 1, log: false) { }

        /// <summary>
        /// Performs final cleanup for entities marked for destruction, sending despawn packets and removing from world.
        /// </summary>
        /// <param name="ntt">The entity to destroy</param>
        /// <param name="def">Destroy end of frame component (marker for destruction)</param>
        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
        {
            if (ntt.IsPlayer() || ntt.IsMonster())
            {
                var despawnPacket = MsgAction.RemoveEntity(ntt.Id);
                ntt.NetSync(ref despawnPacket, true);
            }
            else if (ntt.IsItem())
            {
                var deletePacket = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                ntt.NetSync(ref deletePacket, true);
            }

            NttWorld.Destroy(ntt);

            if (IsLogging)
                FConsole.WriteLine("Destroyed {ntt}", ntt);
        }
    }
}