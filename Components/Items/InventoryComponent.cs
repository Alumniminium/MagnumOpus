using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
    public struct InventoryComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public NTT[] Items;

        private uint money;
        public uint Money
        {
            get => money; 
            set
            {
                money = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, money, MsgUserAttribType.MoneyInventory);
                entity.NetSync(ref packet, true);
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
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, cps, MsgUserAttribType.CPsInventory);
                entity.NetSync(ref packet, true);
            }
        }

        public InventoryComponent(int entityId, uint money, uint cps)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Money = money;
            CPs = cps;
            Items = new NTT[40];
        }

        public override int GetHashCode() => EntityId;
    }
}