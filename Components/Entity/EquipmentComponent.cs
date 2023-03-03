using System.Text.Json.Serialization;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct EquipmentComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        [JsonIgnore]
        public NTT Head => Items[MsgItemPosition.Armor];
        [JsonIgnore]
        public NTT Necklace => Items[MsgItemPosition.Necklace];
        [JsonIgnore]
        public NTT Garment => Items[MsgItemPosition.Garment];
        [JsonIgnore]
        public NTT Bottle => Items[MsgItemPosition.Bottle];
        [JsonIgnore]
        public NTT Armor => Items[MsgItemPosition.Armor];
        [JsonIgnore]
        public NTT Ring => Items[MsgItemPosition.Ring];
        [JsonIgnore]
        public NTT MainHand => Items[MsgItemPosition.RightWeapon];
        [JsonIgnore]
        public NTT OffHand => Items[MsgItemPosition.LeftWeapon];
        [JsonIgnore]
        public NTT Boots => Items[MsgItemPosition.Boots];
        
        public Dictionary<MsgItemPosition, NTT> Items = new()
        {
                { MsgItemPosition.Head, default },
                { MsgItemPosition.Necklace, default },
                { MsgItemPosition.Garment, default },
                { MsgItemPosition.Bottle, default },
                { MsgItemPosition.Armor, default },
                { MsgItemPosition.Ring, default },
                { MsgItemPosition.RightWeapon, default },
                { MsgItemPosition.LeftWeapon, default },
                { MsgItemPosition.Boots, default }
            };

        public EquipmentComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}