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
            var ntc = new NameTagComponent(ntt.Id, net.Username);
            var bdy = new BodyComponent(ntt.Id, (uint)(net.Username == "trbl" ? 2003 : 2002));
            var hed = new HeadComponent(ntt.Id, 6);
            var emo = new EmoteComponent(ntt.Id, Emote.Stand);
            var vwp = new ViewportComponent(ntt.Id, 40);
            var pos = new PositionComponent(ntt.Id, new System.Numerics.Vector2(438,377), 1002);
            var eff = new StatusEffectComponent(ntt.Id);
            var inv = new InventoryComponent(ntt.Id, 1000, 0);
            var lvl = new LevelComponent(ntt.Id, 1);
            var hlt = new HealthComponent(ntt.Id, 330, 330);
            var mana = new ManaComponent(ntt.Id, 1000, 1000);
            var pro = new ClassComponent(ntt.Id, ClasseName.Archer);
            var sbc = new SpellBookComponent(ntt.Id);
            var eqc = new EquipmentComponent(ntt.Id);

            sbc.Spells.Add(1000, (4, 0, 0));
            sbc.Spells.Add(1005, (4, 0, 0));
            sbc.Spells.Add(1120, (3, 0, 0));
            sbc.Spells.Add(1165, (3, 0, 0));
            sbc.Spells.Add(1166, (3, 0, 0));
            sbc.Spells.Add(1167, (3, 0, 0));
            sbc.Spells.Add(1045, (4, 0, 0));
            sbc.Spells.Add(8001, (5, 0, 0));

            ntt.Set(ref bdy);
            ntt.Set(ref ntc);
            ntt.Set(ref vwp);
            ntt.Set(ref emo);
            ntt.Set(ref pos);
            ntt.Set(ref eff);
            ntt.Set(ref sbc);
            ntt.Set(ref inv);
            ntt.Set(ref lvl);
            ntt.Set(ref pro);
            ntt.Set(ref hlt);
            ntt.Set(ref mana);
            ntt.Set(ref eqc);
            ntt.Set(ref hed);
            
            var testItem = PixelWorld.CreateEntity(EntityType.Item);
            var itemComp = new ItemComponent(testItem.Id, 1001020, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            testItem.Set(ref itemComp);
            inv.Items[0] = testItem;

            var testItem2 = PixelWorld.CreateEntity(EntityType.Item);
            var itemComp2 = new ItemComponent(testItem2.Id, 1000000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            testItem2.Set(ref itemComp2);
            inv.Items[1] = testItem2;

            var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
            var info = MsgCharacter.Create(ntt);
            var msgStatus = MsgMapStatus.Create(1002,(uint)(MapFlags.NewbieProtect | MapFlags.NoPk));

            ntt.NetSync(ref ok);
            ntt.NetSync(ref info);
            ntt.NetSync(ref msgStatus);
        }
    }
}