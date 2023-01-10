using System.Globalization;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Components;

namespace MagnumOpus.Networking
{
    public static unsafe class LoginPacketHandler
    {
        internal static void Process(in PixelEntity ntt, in Memory<byte> packet)
        {
            var id = BitConverter.ToUInt16(packet.Span[2..]);

            switch (id)
            {
                case 1051:
                    {
                        var msgAccount = Co2Packet.Deserialze<MsgAccount>(packet);
                        var username = msgAccount.GetUsername();
                        RivestCipher5.Decrypt(msgAccount.Password, 16);
                        var password = msgAccount.GetPassword();
                        var server = msgAccount.GetServer();

                        FConsole.WriteLine($"[LOGIN/1051] Account: {username}, Pass: {password}, Server: {server}");

                        var response = MsgAccountResponse.Create("62.178.176.71", 5816, (uint)ntt.Id, (uint)ntt.Id);
                        ref var net = ref ntt.Get<NetworkComponent>();
                        net.Username = username;
                        ntt.NetSync(ref response);
                        break;
                    }
                case 1052:
                    {
                        var msg = Co2Packet.Deserialze<MsgConnectLogin>(packet);
                        
                        var player = PixelWorld.GetEntity((int)msg.UniqueId);
                        var filename = msg.GetFileName();
                        FConsole.WriteLine($"[LOGIN/1052] Client Id: {msg.UniqueId}, File: {filename} Contents: {msg.Contents}");

                        ref readonly var net = ref player.Get<NetworkComponent>();
                        var ntc = new NameTagComponent(player.Id, "test");
                        player.Set(ref ntc);
                        break;
                    }
                default:
                    {
                        FConsole.WriteLine($"[LOGIN/{id}] Unknown packet");
                        // ref var net = ref ntt.Get<NetworkComponent>();
                        // net.Socket.Close();
                        // net.Socket.Dispose();
                        FConsole.WriteLine(packet.Dump());
                        break;
                    }
            }
        }
    }
}