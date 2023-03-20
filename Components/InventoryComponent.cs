using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components
{



    [Component(saveEnabled: true)]
    public struct InventoryComponent
    {
        public NTT NTT;
        public long ChangedTick;

        public Memory<NTT> Items = new NTT[40];

        private uint money;
        public uint Money
        {
            get => money;
            set
            {
                money = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, money, MsgUserAttribType.MoneyInventory);
                NTT.NetSync(ref packet, true);
            }
        }

        private uint cps;
        public uint CPs
        {
            get => cps;
            set
            {
                cps = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, cps, MsgUserAttribType.CPsInventory);
                NTT.NetSync(ref packet, true);
            }
        }

        public InventoryComponent(in NTT ntt, uint money, uint cps)
        {
            NTT = ntt;
            ChangedTick = NttWorld.Tick;
            Money = money;
            CPs = cps;

            Items = new NTT[40];
            for (var i = 0; i < Items.Length; i++)
                Items.Span[i] = new(0, EntityType.Other);
        }

        public override int GetHashCode() => NTT;
    }
}