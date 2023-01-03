using System.Runtime.InteropServices;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgLogin
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public ulong Token;
        [FieldOffset(4)]
        public uint UniqueId;
        [FieldOffset(12)]
        public ushort ClientVersion;
        [FieldOffset(14)]
        public fixed byte Language[10];
        [FieldOffset(24)]
        public uint Contents;

        public string GetLanguage()
        {
            fixed (byte* ptr = Language)
                return Encoding.ASCII.GetString(ptr, 10).Trim('\0');
        }

        [PacketHandler(PacketId.MsgLogin)]
        public static void Process(PixelEntity ntt, Memory<byte> packet)
        {
            var msg = Co2Packet.Deserialze<MsgLogin>(packet);
            var language = msg.GetLanguage();
            FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");
            ref readonly var net = ref ntt.Get<NetworkComponent>();
            var ntc = new NameTagComponent(ntt.NetId, net.Username);
            var dir = new DirectionComponent(ntt.Id, Direction.South);
            var emo = new EmoteComponent(ntt.Id, Emote.Dance);
            var vwp = new ViewportComponent(ntt.Id, 40);
            var syn = new NetSyncComponent(ntt.Id, SyncThings.All);
            var pos = new PositionComponent(ntt.Id, new System.Numerics.Vector2(438,377), 1002);
            var eff = new StatusEffectComponent(ntt.Id);
            var inv = new InventoryComponent(ntt.Id, 1000, 0);
            var lvl = new LevelComponent(ntt.Id, 1);
            var pro = new ProfessionComponent(ntt.Id, ClasseName.InternTaoist);
            var sbc = new SpellBookComponent(ntt.Id);
            sbc.Spells.Add(1000, (0, 0, 0));
            sbc.Spells.Add(1005, (0, 0, 0));
            ntt.Add(ref ntc);
            ntt.Add(ref dir);
            ntt.Add(ref vwp);
            ntt.Add(ref syn);
            ntt.Add(ref emo);
            ntt.Add(ref pos);
            ntt.Add(ref eff);
            ntt.Add(ref sbc);
            ntt.Add(ref inv);
            ntt.Add(ref lvl);
            ntt.Add(ref pro);

            var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
            var okserialized = Co2Packet.Serialize(ref ok, ok.Size);
            ntt.NetSync(okserialized);

            var info = MsgCharacter.Create(ntt);
            var serialized = Co2Packet.Serialize(ref info, info.Size);
            ntt.NetSync(serialized);

            var msgStatus = MsgStatus.Create(1002,3282567244);
            ntt.NetSync(msgStatus);
        }
    }
}