using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Networking
{
    public static class CqPortalProcessor
    {
        private class PasswayDef
        {
            public int Identity { get; set; }
            public int MapIndex { get; set; }
            public int MapId { get; set; }
            public int TargetMapId { get; set; }
            public int TargetPortal { get; set; }
        }
        private class PortalDef
        {
            public int Id { get; set; }
            public int PasswayIndex { get; set; }
            public int PasswayMapIndex { get; set; }
            public int PasswayTargetMapId { get; set;}
            public int PortalMapId { get; set; }
            public int PortalX { get; set; }
            public int PortalY { get; set; }
        }
        private static readonly Dictionary<int, PasswayDef> PasswayDefs = new();
        private static readonly Dictionary<int, PortalDef> PortalDefs = new();

        public static void Load()
        {
            using var ctx = new SquigglyContext();
            var passways = ctx.cq_passway.ToArray();

            foreach(var passway in passways)
            {
                var passwayDef = new PasswayDef
                {
                    Identity = (int)passway.id,
                    MapIndex = (int)passway.passway_idx,
                    MapId = (int)passway.mapid,
                    TargetMapId = (int)passway.passway_mapid,
                    TargetPortal = (int)passway.passway_mapportal
                };
                PasswayDefs.Add(passwayDef.Identity, passwayDef);
            }

            foreach(var passway in PasswayDefs)
            {
                var portal = ctx.cq_portal.FirstOrDefault(x => x.mapid == passway.Value.TargetMapId && x.portal_idx == passway.Value.TargetPortal);
                var portalDef = new PortalDef
                {
                    Id = (int)portal.id,
                    PasswayIndex = passway.Value.MapIndex,
                    PasswayTargetMapId = passway.Value.MapId,
                    PortalMapId = (int)portal.mapid,
                    PortalX = (int)portal.portal_x,
                    PortalY = (int)portal.portal_y
                };
                PortalDefs.Add(portalDef.Id, portalDef);
            }
        }
    }
    public static class ItemInfo
    {
        public static bool IsBroken(ref ItemComponent item) => item.CurrentDurability == 0;
        public static bool IsNeverDropWhenDead(ref ItemComponent item) => /*item.Monopoly == 3 || item..Monopoly == 3 || item..Monopoly == 9 || */ item.Plus > 5;
        public static int GetLevel(ref ItemComponent item) => (int)(item.Id % 1000 / 10);
        public static int GetLevel(int nType) => nType % 1000 / 10;
        public static ref ItemComponent SetLevel(ref ItemComponent item, int level)
        {
            item.Id = item.Id / 1000 * 1000 + level * 10 + item.Id % 10;
            return ref item;
        }
        public static int GetQuality(ref ItemComponent item) => item.Id % 10;
        public static int GetQuality(int nType) => nType % 10;
        public static ref ItemComponent SetQuality(ref ItemComponent item, int quality)
        {
            item.Id = item.Id / 10 * 10 + quality;
            return ref item;
        }
        public static int GetItemSubtype(ref ItemComponent item) => item.Id / 1000;
        public static int GetItemSubtype(int nType) => nType / 1000;
        public static bool IsArrowSort(ref ItemComponent item) => item.Id / 1000 == 1050;
        public static bool IsArrowSort(int nType) => nType / 1000 == 1050;
        public static bool IsArrow(ref ItemComponent item) => IsArrowSort(ref item);
        public static bool IsArrow(int nType) => IsArrowSort(nType);
        public static bool IsExpend(ref ItemComponent item) => IsArrowSort(ref item) || item.Id < 50000;
        public static bool IsExpend(int nType) => IsArrowSort(nType) || nType < 50000;
        public static bool IsBow(ref ItemComponent item) => item.Id / 1000 == 500;
        public static bool IsBow(int nType) => nType / 1000 == 500;
        public static bool IsCountable(ref ItemComponent item) => IsArrowSort(ref item);
        public static bool IsCountable(int nType) => IsArrowSort(nType);
        public static bool IsEquipment(ref ItemComponent item) => !IsArrowSort(ref item) && (int)GetSort(ref item) < 7 || IsShield(ref item) || GetItemSubtype(ref item) == 2100;
        public static bool IsEquipment(int nType) => !IsArrowSort(nType) && (int)GetSort(nType) < 7 || IsShield(nType);
        public static bool IsArmor(ref ItemComponent item) => item.Id / 10000 == 13;
        public static bool IsArmor(int nType) => nType / 10000 == 13;
        public static bool IsShield(int nType) => nType / 1000 == 900;
        public static bool IsShield(ref ItemComponent item) => item.Id / 1000 == 900;
        public static ItemSort GetSort(ref ItemComponent item) => (ItemSort)(item.Id / 100000);
        public static ItemSort GetSort(int nType) => (ItemSort)(nType / 100000);
    }
    public static class ItemGenerator
    {
        private static readonly ushort[] NecklaceType = { 120, 121 };//120 necklace 121 bags
        private static readonly ushort[] RingType = { 150, 151, 152 };//150 att 151 agi 152 bracelets
        private static readonly ushort[] ArmetType = { 111, 112, 113, 114, 117, 118 };
        private static readonly ushort[] ArmorType = { 130, 131, 132, 133, 134 };
        private static readonly ushort[] OneHanderType = { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500 };//601 was here, but doesn't seem to be valid?
        private static readonly ushort[] TwoHanderType = { 510, 530, 560, 561, 580, 900 };

        public static ItemComponent Generate(cq_monstertype monster)
        {
            var entry = new ItemComponent();
            var possibleTypes = new List<ushort>();
            if(monster.drop_armet != 0)
                possibleTypes.AddRange(ArmetType);
            if (monster.drop_armor != 0)
                possibleTypes.AddRange(ArmorType);
            if (monster.drop_necklace != 0)
                possibleTypes.AddRange(NecklaceType);
            if (monster.drop_ring != 0)
                possibleTypes.AddRange(RingType);
            if (monster.drop_weapon != 0)
                possibleTypes.AddRange(OneHanderType);
            // if (monster.drop_twohander != 0)        
            //     possibleTypes.AddRange(TwoHanderType);

            var type = possibleTypes[Random.Shared.Next(0, possibleTypes.Count)];
            entry.Id = type * 1000 + Random.Shared.Next(1, 100) * 10 + Random.Shared.Next(1, 10);
            entry.CurrentDurability = entry.MaximumDurability = 100;
            entry = AddBless(ref entry);
            entry = AddPlus(ref entry);
            entry = AddSockets(ref entry);
            return entry;
        }


        public static ref ItemComponent AddBless(ref ItemComponent entry)
        {
            var dice = Random.Shared.NextSingle();
            if (dice < 0.01)
                entry.Bless = 1;
            if (dice < 0.001)
                entry.Bless = 3;
            if (dice < 0.0001)
                entry.Bless = 7;
            return ref entry;
        }

        public static ref ItemComponent AddPlus(ref ItemComponent entry)
        {
            var dice = Random.Shared.NextSingle();
            if (dice < 0.01)
                entry.Plus = 1;
            return ref entry;
        }

        public static ref ItemComponent AddSockets(ref ItemComponent entry)
        {
            var dice = Random.Shared.NextSingle();
            if (dice < 0.01)
                entry.Gem1 = 255;
            if (dice < 0.001)
                entry.Gem2 = 255;
            return ref entry;
        }

        private static ref ItemComponent GenerateQuality(ref ItemComponent entry, int moblevel)
        {
            var q = Random.Shared.Next(1, 1000);
            q -= moblevel / 25;

            if (q < 1)
                q= 9;//super
            else if (q < 11)
                q= 8;//eli
            else if (q < 31)
                q= 7;//uni
            else if( q < 55)
                q= 6;//refined
            else if (q < 75)
                q= 5;//Fixed 
            else if (q < 95)
                q= 4;//normal
            else if (q < 115)
                q= 3;//normal

            entry.Id = entry.Id / 10 * 10 + q;
            return ref entry;
        }
    }
}