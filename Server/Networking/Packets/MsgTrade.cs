using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTrade
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public TradeMode Mode;

        public static MsgTrade Create(int uniqueId, TradeMode mode)
        {
            var packet = new MsgTrade
            {
                Size = (ushort)sizeof(MsgTrade),
                Id = 1056,
                UniqueId = uniqueId,
                Mode = mode,
            };
            return packet;
        }

        [PacketHandler(PacketId.MsgTrade)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialize<MsgTrade>(memory.Span);
            if (ntt.Id != msg.UniqueId)
                FConsole.WriteLine($"[{nameof(MsgTrade)}] UID Mismatch! Packet: {msg.UniqueId}, ntt: {ntt.Id}");

            switch (msg.Mode)
            {
                //     case TradeMode.RequestNewTrade:
                //         break;
                //     case TradeMode.RequestCloseTrade:
                //         break;
                //     case TradeMode.ShowTradeWindow:
                //         break;
                //     case TradeMode.CloseTradeWindowSuccess:
                //         break;
                //     case TradeMode.CloseTradeWindowFail:
                //         break;
                //     case TradeMode.RequestAddItemToTrade:
                //         break;
                //     case TradeMode.RequestAddMoneyToTrade:
                //         break;
                //     case TradeMode.DisplayMoney:
                //         break;
                //     case TradeMode.DisplayMoneyAdd:
                //         break;
                //     case TradeMode.RequestCompleteTrade:
                //         break;
                //     case TradeMode.ReturnItem:
                //         break;
                default:
                    FConsole.WriteLine($"[{nameof(MsgTrade)}] Unknown TradeMode: {msg.Mode}");
                    break;
            }
        }
    }
}