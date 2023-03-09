using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.Components.Entity;
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
            var packetType = (PacketId)BitConverter.ToUInt16(packet.Span[2..]);

            switch (packetType)
            {
                case PacketId.MsgConnect:
                    {
                        var msgAccount = Co2Packet.Deserialze<MsgAccount>(packet);
                        var username = msgAccount.GetUsername();
                        RivestCipher5.Decrypt(msgAccount.Password, 16);
                        var password = msgAccount.GetPassword();
                        var server = msgAccount.GetServer();

                        FConsole.WriteLine($"[LOGIN/1051] Account: {username}, Pass: {password}, Server: {server}");

                        var response = MsgAccountResponse.Create(Constants.SERVERIP, 5816, ntt.Id, ntt.Id);
                        ref var net = ref ntt.Get<NetworkComponent>();
                        net.Username = username;
                        ntt.NetSync(ref response);
                        break;
                    }
                case PacketId.MsgLogin:
                    {
                        var msg = Co2Packet.Deserialze<MsgConnectLogin>(packet);

                        var player = NttWorld.GetEntity((int)msg.UniqueId);
                        var filename = msg.GetFileName();
                        FConsole.WriteLine($"[LOGIN/1052] Client Id: {msg.UniqueId}, File: {filename} Contents: {msg.Contents}");

                        ref readonly var net = ref player.Get<NetworkComponent>();
                        var ntc = new NameTagComponent(player.Id, "test");
                        player.Set(ref ntc);
                        break;
                    }

                case PacketId.MsgRole:
                    break;
                case PacketId.MsgText:
                    break;
                case PacketId.MsgWalk:
                    break;
                case PacketId.MsgHero:
                    break;
                case PacketId.MsgItem:
                    break;
                case PacketId.MsgTick:
                    break;
                case PacketId.MsgAction:
                    break;
                case PacketId.MsgSpwan:
                    break;
                case PacketId.MsgName:
                    break;
                case PacketId.MsgUpdate:
                    break;
                case PacketId.MsgFriend:
                    break;
                case PacketId.MsgInteract:
                    break;
                case PacketId.MsgTeam:
                    break;
                case PacketId.MsgSockEm:
                    break;
                case PacketId.MsgForge:
                    break;
                case PacketId.MsgTime:
                    break;
                case PacketId.MsgTransfer:
                    break;
                case PacketId.MsgTrade:
                    break;
                case PacketId.MsgFloorItem:
                    break;
                case PacketId.MsgStorage:
                    break;
                case PacketId.MsgMagicEffect:
                    break;
                case PacketId.MsgGuild:
                    break;
                case PacketId.MsgNpc:
                    break;
                case PacketId.MsgGuildInfo:
                    break;
                case PacketId.MsgNpcSpawn:
                    break;
                case PacketId.MsgDialog:
                    break;
                case PacketId.MsgDialog2:
                    break;
                case PacketId.MsgCompose:
                    break;
                default:
                    {
                        FConsole.WriteLine($"[LOGIN/{(int)packetType}/{packetType}] Unknown packet");
                        FConsole.WriteLine(packet.Dump());
                        break;
                    }
            }
        }
    }
}