using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class LoginPacketHandler
    {
        internal static void Process(in NTT ntt, in Memory<byte> packet)
        {
            try
            {
                var packetType = (PacketId)BitConverter.ToUInt16(packet.Span[2..]);

                switch (packetType)
                {
                    case PacketId.MsgConnect:
                        {
                            var msgAccount = Co2Packet.Deserialize<MsgAccount>(packet.Span);
                            var username = msgAccount.GetUsername();
                            RivestCipher5.Decrypt(msgAccount.Password, 16);
                            var password = msgAccount.GetPassword();
                            var server = msgAccount.GetServer();

                            FConsole.WriteLine($"[LOGIN/1051] Account: {username}, Pass: {password}, Server: {server}");

                            var response = MsgAccountResponse.Create(Constants.ServerIP, Constants.GamePort, ntt.Id, ntt.Id);
                            ref var net = ref ntt.Get<NetworkComponent>();
                            net.Username = username;
                            ntt.NetSync(ref response);
                            break;
                        }
                    case PacketId.MsgLogin:
                        {
                            var msg = Co2Packet.Deserialize<MsgConnectLogin>(packet.Span);
                            var filename = msg.GetFileName();
                            FConsole.WriteLine($"[LOGIN/1052] Client Id: {msg.UniqueId}, File: {filename} Contents: {msg.Contents}");
                            break;
                        }
                    default:
                        {
                            FConsole.WriteLine($"[LOGIN/{(int)packetType}/{packetType}] Unknown packet");
                            FConsole.WriteLine(packet.Dump());
                            break;
                        }
                }
            }
            catch
            {
                NttWorld.Destroy(ntt);
            }
        }
    }
}