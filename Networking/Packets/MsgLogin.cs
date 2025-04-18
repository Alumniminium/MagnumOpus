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
        public static void Process(NTT ntt, Memory<byte> packet)
        {
            var msg = Co2Packet.Deserialize<MsgLogin>(packet.Span);
            var language = msg.GetLanguage();
            FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");

            PrometheusPush.LoginCount.Inc();

            ref var net = ref ntt.Get<NetworkComponent>();

            var found = false;
            foreach (var kvp in NttWorld.NTTs)
            {
                var oldNtt = kvp.Value;
                ref var oldNtc = ref oldNtt.Get<NameTagComponent>();

                if (oldNtc.Name == net.Username)
                {
                    oldNtt.ChangeOwner(ntt);
                    // ntt.Remove<NetworkComponent>();
                    // ntt.Set<DestroyEndOfFrameComponent>();
                    found = true;
                    // ntt.Id = oldNtt.Id;
                    FConsole.WriteLine($"Found NTT with Id: {oldNtt.Id}!");
                    break;
                }
            }

            if (!found)
            {
                var ntc = new NameTagComponent(net.Username);
                var bdy = new BodyComponent(in ntt, (uint)(net.Username == "trbl" ? 2003 : 2002));
                var hed = new HeadComponent(in ntt);
                var emo = new EmoteComponent(Emote.Stand);
                var pos = new PositionComponent(new System.Numerics.Vector2(438, 377), 1002);
                var eff = new StatusEffectComponent(in ntt);
                var inv = new InventoryComponent(in ntt, 1000000, 0);
                var lvl = new LevelComponent(in ntt);
                var hlt = new HealthComponent(ntt, 330, 330);
                var mana = new ManaComponent(1000, 1000);
                var pro = new ProfessionComponent(ntt, ClasseName.Archer);
                var sbc = new SpellBookComponent();
                var eqc = new EquipmentComponent();
                var stm = new StaminaComponent(ntt);

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
                ntt.Set(ref stm);

                var testItem = NttWorld.CreateEntity(EntityType.Item);
                var itemComp = new ItemComponent(1001020, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                testItem.Set(ref itemComp);
                inv.Items.Span[0] = testItem;

                var testItem2 = NttWorld.CreateEntity(EntityType.Item);
                var itemComp2 = new ItemComponent(1000000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                testItem2.Set(ref itemComp2);
                inv.Items.Span[1] = testItem2;
            }

            var vwp = new ViewportComponent(21);
            ntt.Set(ref vwp);

            var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
            var info = MsgCharacter.Create(ntt);
            var msgMap = MsgMapStatus.Create(1002, (uint)(MapFlags.Mine | MapFlags.NewbieProtect));

            ntt.NetSync(ref ok);
            ntt.NetSync(ref info);
            ntt.NetSync(ref msgMap);
        }
    }
}