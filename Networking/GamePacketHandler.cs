using System.Globalization;
using System.Text;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking
{
    public static unsafe class GamePacketHandler
    {
        internal static void Process(in PixelEntity ntt, Memory<byte> packet)
        {
            var size = BitConverter.ToUInt16(packet.Span[0..]);
            var id = BitConverter.ToUInt16(packet.Span[2..]);
            ref readonly var net = ref ntt.Get<NetworkComponent>();

            Console.WriteLine($"[GAME] Received packet {id} with size {size} | actual buffer size: {packet.Length}");
            FConsole.WriteLine(Dump(packet.ToArray()));

            switch (id)
            {
                case 1052:
                    {
                        var msg = (MsgConnectGame)packet;
                        var language = msg.GetLanguage();
                        FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");
                        var ntc = new NameTagComponent(ntt.Id, "test");
                        ntt.Add(ref ntc);

                        var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", Enums.MsgTextType.LoginInformation);
                        net.GameCrypto.Encrypt(ok, ok);
                        ntt.NetSync(ok);

                        var info = MsgCharacter.Create(ntt).ToArray();
                        net.GameCrypto.Encrypt(info, info);
                        ntt.NetSync(info);

                        break;
                    }
                case 1001:
                    {
                        // var msg = (MsgRole)packet;
                        var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", Enums.MsgTextType.LoginInformation);
                        net.GameCrypto.Encrypt(ok, ok);
                        ntt.NetSync(ok);
                        break;
                    }
                case 1010:
                    {
                        var msg = (MsgAction)packet;

                        switch (msg.Type)
                        {
                            case Enums.MsgActionType.SendLocation:
                                {
                                    var reply = MsgAction.Create(0, ntt.Id, 1002, 438, 377, 0, msg.Type);
                                    net.GameCrypto.Encrypt(reply.Span, reply.Span);
                                    ntt.NetSync(in reply);
                                    break;
                                }
                            case Enums.MsgActionType.Jump:
                            {
                                ref var pos = ref ntt.Get<PositionComponent>();
                                
                                break;
                            }
                            default:
                                {
                                    FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {msg.Type}");
                                    var reply = MsgAction.Create(msg.Timestamp, ntt.Id, msg.Param, msg.X, msg.Y, msg.Direction, msg.Type);
                                    var tqServer = Encoding.ASCII.GetBytes("TQServer");
                                    tqServer.CopyTo(packet.Span[24..]);
                                    net.GameCrypto.Encrypt(reply.Span, reply.Span);
                                    ntt.NetSync(in reply);
                                    // net.GameCrypto.Encrypt(packet.Span,packet.Span);
                                    // ntt.NetSync(in packet);
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        FConsole.WriteLine($"[GAME] Unknown packet ID: {id}");
                        break;
                    }
            }
        }
        public static string Dump(byte[] Bytes)
        {
            string Hex = "";
            foreach (byte b in Bytes)
            {
                Hex = Hex + b.ToString("X2") + " ";
            }
            string Out = "";
            while (Hex.Length != 0)
            {
                int SubLength = Hex.Length >= 48 ? 48 : Hex.Length;
                string SubString = Hex[..SubLength];
                int Remove = SubString.Length;
                SubString = SubString.PadRight(60, ' ') + StrHexToAnsi(SubString);
                Hex = Hex.Remove(0, Remove);
                Out = Out + SubString + "\r\n";
            }
            return Out;
        }

        private static string StrHexToAnsi(string StrHex)
        {
            string[] Data = StrHex.Split(new char[] { ' ' });
            string Ansi = "";
            foreach (string tmpHex in Data)
            {
                if (tmpHex != "")
                {
                    byte ByteData = byte.Parse(tmpHex, NumberStyles.HexNumber);
                    if ((ByteData >= 32) & (ByteData <= 126))
                        Ansi += ((char)ByteData).ToString();
                    else
                        Ansi += ".";
                }
            }
            return Ansi;
        }
    }
}