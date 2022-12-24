using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1 )]
    public unsafe struct MsgItemInfoEx
    {
        [FieldOffset(0)]
        public readonly ushort Size;//0
        [FieldOffset(2)]
        public readonly ushort Id;//2
        [FieldOffset(4)]
        public readonly int UniqueId;//4
        [FieldOffset(8)]
        public int OwnerUID;//8
        [FieldOffset(12)]
        public int Price;//12
        [FieldOffset(16)]
        public readonly int ItemId;//16
        [FieldOffset(20)]
        public readonly ushort CurrentDurability;//20
        [FieldOffset(22)]
        public readonly ushort MaxiumumDurability;//22
        [FieldOffset(24)]
        public readonly ItemExType ItemExType;//24
        [FieldOffset(25)]
        public readonly byte Ident;//25
        [FieldOffset(26)]
        public readonly MsgItemPosition Position;//26
        [FieldOffset(27)]
        public readonly int Unknow1;//27
        //public readonly byte Dunno;//31
        [FieldOffset(32)]
        public readonly byte Gem1;
        [FieldOffset(33)]
        public readonly byte Gem2;
        [FieldOffset(34)]
        public readonly RebornItemEffect RebornEffect;
        [FieldOffset(35)]
        public readonly byte Magic2;
        [FieldOffset(36)]
        public readonly byte Plus;
        [FieldOffset(37)]
        public readonly byte Bless;
        [FieldOffset(38)]
        public readonly byte Enchant;
        [FieldOffset(39)]
        public readonly int Restrain;

        // public MsgItemInfoEx(Item item, MsgItemPosition position = MsgItemPosition.Inventory, ItemExType type = ItemExType.None) : this()
        // {
        //     Size = (ushort)sizeof(MsgItemInfoEx);
        //     Id = 1108;
        //     OwnerUID = item.OwnerUniqueId;
        //     UniqueId = item.UniqueId;
        //     ItemId = item.ItemId;
        //     Price = item.PriceBaseline;
        //     CurrentDurability = item.CurrentDurability;
        //     MaxiumumDurability = item.MaximumDurability;
        //     ItemExType = type;
        //     Position = position;
        //     Gem1 = item.Gem1;
        //     Gem2 = item.Gem2;
        //     RebornEffect = item.RebornEffect;
        //     Plus = item.Plus;
        //     Bless = item.Bless;
        //     Enchant = item.Enchant;
        //     Restrain = item.CustomTextId;
        // }

        // public static MsgItemInfoEx CreateBoothItem(Item item)
        // {
        //     var msg = new MsgItemInfoEx(item, MsgItemPosition.Inventory, ItemExType.Booth)
        //     {
        //         OwnerUID = item.Product.ShopId,
        //         Price = item.Product.Price
        //     };
        //     return msg;
        // }
    }
}