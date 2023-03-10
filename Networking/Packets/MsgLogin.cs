using System.Runtime.InteropServices;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Leveling;
using MagnumOpus.Components.Location;
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
        public static void Process(NTT ntt, Memory<byte> packet)
        {
            var msg = Co2Packet.Deserialze<MsgLogin>(packet);
            var language = msg.GetLanguage();
            FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");

            PrometheusPush.LoginCount.Inc();

            ref readonly var net = ref ntt.Get<NetworkComponent>();
            var found = false;
            foreach (var kvp in NttWorld.NTTs)
            {
                var oldNtt = kvp.Value;
                ref var oldNtc = ref oldNtt.Get<NameTagComponent>();

                if (oldNtc.Name == net.Username)
                {
                    oldNtt.Set(net);
                    // ntt.Remove<NetworkComponent>();
                    // ntt.Set<DestroyEndOfFrameComponent>();
                    found = true;
                    ntt = oldNtt;
                    break;
                }
            }
            if (!found)
            {
                var ntc = new NameTagComponent(net.Username);
                var bdy = new BodyComponent(ntt.Id, (uint)(net.Username == "trbl" ? 2003 : 2002));
                var hed = new HeadComponent(6);
                var emo = new EmoteComponent(Emote.Stand);
                var vwp = new ViewportComponent(21);
                var pos = new PositionComponent(new System.Numerics.Vector2(438, 377), 1002);
                var eff = new StatusEffectComponent(ntt.Id);
                var inv = new InventoryComponent(ntt.Id, 1000000, 0);
                var lvl = new LevelComponent(1);
                var hlt = new HealthComponent(ntt, 330, 330);
                var mana = new ManaComponent(1000, 1000);
                var pro = new ProfessionComponent(ntt, ClasseName.Archer);
                var sbc = new SpellBookComponent();
                var eqc = new EquipmentComponent();

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

                var testItem = NttWorld.CreateEntity(EntityType.Item);
                var itemComp = new ItemComponent(1001020, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                testItem.Set(ref itemComp);
                inv.Items[0] = testItem;

                var testItem2 = NttWorld.CreateEntity(EntityType.Item);
                var itemComp2 = new ItemComponent(1000000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                testItem2.Set(ref itemComp2);
                inv.Items[1] = testItem2;
            }
            var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
            var info = MsgCharacter.Create(ntt);
            //(MapFlags.EnablePlayerShop | MapFlags.Mine | MapFlags.NewbieProtect)
            var msgMap = MsgMapStatus.Create(1002, (uint)0);

            ntt.NetSync(ref ok);
            ntt.NetSync(ref info);
            ntt.NetSync(ref msgMap);
        }
    }
}