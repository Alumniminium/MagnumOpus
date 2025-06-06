using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy", threads: 2, log: false) { }

        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def)
        {
            switch (ntt.Type)
            {
                case EntityType.Player:
                case EntityType.Monster:
                    var despawn = MsgAction.RemoveEntity(ntt.Id);
                    ntt.NetSync(ref despawn, true);
                    break;
                case EntityType.Item:
                    var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                    ntt.NetSync(ref delete, true);
                    break;
            }

            NttWorld.Destroy(ntt);

            if (IsLogging)
                FConsole.WriteLine("Destroyed {ntt}", ntt);
        }
    }
}