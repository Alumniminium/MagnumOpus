using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;
using Org.BouncyCastle.Utilities.Encoders;

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
                        var msgAccount = (MsgAccount)packet;
                        var username = msgAccount.GetUsername();
                        RivestCipher5.Decrypt(msgAccount.Password, 16);
                        var password = msgAccount.GetPassword();
                        var server = msgAccount.GetServer();

                        FConsole.WriteLine($"[LOGIN/1051] Account: {username}, Pass: {password}, Server: {server}");

                        Memory<byte> response = MsgAccountResponse.Create("192.168.0.10", 5816, (uint)ntt.Id, (uint)ntt.Id);
                        ref readonly var net = ref ntt.Get<NetworkComponent>();
                        net.AuthCrypto.Encrypt(response.Span, response.Span);
                        ntt.NetSync(response);
                        break;
                    }
                case 1052:
                    {
                        var msg = (MsgConnectLogin)packet;
                        
                        var player = PixelWorld.GetEntity((int)msg.UniqueId);
                        var filename = msg.GetFileName();
                        FConsole.WriteLine($"[LOGIN/1052] Client Id: {msg.UniqueId}, File: {filename}");

                        ref readonly var net = ref player.Get<NetworkComponent>();
                        var ntc = new NameTagComponent(player.Id, "test");
                        player.Add(ref ntc);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"[LOGIN/{id}] Unknown packet");
                        // ref var net = ref ntt.Get<NetworkComponent>();
                        // net.Socket.Close();
                        // net.Socket.Dispose();
                        FConsole.WriteLine(Dump(packet.ToArray()));
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