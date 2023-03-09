using System.Runtime.InteropServices;
using MagnumOpus.Components.Items;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgItemInformation
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int ItemId;
        public ushort CurrentDurability;
        public ushort MaxiumumDurability;
        public MsgItemInfoAction Action;
        public byte Ident;
        public MsgItemPosition Position;
        public int Unknow1;
        public byte Gem1;
        public byte Gem2;
        public RebornItemEffect RebornEffect;
        public byte Magic2;
        public byte Plus;
        public byte Bless;
        public byte Enchant;
        public int Restrain;

        public static MsgItemInformation Create(in NTT ntt, MsgItemInfoAction action = MsgItemInfoAction.AddItem, MsgItemPosition position = MsgItemPosition.Inventory)
        {
            var item = ntt.Get<ItemComponent>();
            var msg = new MsgItemInformation
            {
                Size = (ushort)sizeof(MsgItemInformation),
                Id = 1008,
                UniqueId = ntt.Id,
                ItemId = item.Id,
                CurrentDurability = item.CurrentDurability,
                MaxiumumDurability = item.MaximumDurability,
                Action = action,
                Ident = 0,
                Position = position,
                Gem1 = item.Gem1,
                Gem2 = item.Gem2,
                RebornEffect = item.RebornEffect,
                Magic2 = 0,
                Bless = item.Bless,
                Plus = item.Plus,
                Enchant = item.Enchant,
                Restrain = item.CustomTextId
            };
            return msg;
        }
    }
}