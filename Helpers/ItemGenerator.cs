using System.Text;
using Co2Core.IO;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Helpers
{
    public static class ItemHelper
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
        public static readonly ushort[] NecklaceType = { 120, 121 };//120 necklace 121 bags
        public static readonly ushort[] RingType = { 150, 151, 152 };//150 att 151 agi 152 bracelets
        public static readonly ushort[] ArmetType = { 111, 112, 113, 114, 117, 118 };
        public static readonly ushort[] ArmorType = { 130, 131, 132, 133, 134 };
        public static readonly ushort[] OneHanderType = { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500 };//601 was here, but doesn't seem to be valid?
        public static readonly ushort[] TwoHanderType = { 510, 530, 560, 561, 580, 900 };

        public static ItemComponent Generate(Drops drop, int mobLevel = 0)
        {
            var item = new ItemComponent();
            var possibleTypes = new List<(int, ushort[])>();
            if (drop.Armet != 99)
                possibleTypes.Add((drop.Armet, ArmetType));
            if (drop.Armor != 99)
                possibleTypes.Add((drop.Armor, ArmorType));
            if (drop.Necklace != 99)
                possibleTypes.Add((drop.Necklace, NecklaceType));
            if (drop.Ring != 99)
                possibleTypes.Add((drop.Ring, RingType));
            if (drop.Weapon != 99)
            {
                possibleTypes.Add((drop.Weapon, OneHanderType));
                possibleTypes.Add((drop.Weapon, TwoHanderType));
            }

            if (possibleTypes.Count == 0)
                return default;

            var possibleCombination = possibleTypes[Random.Shared.Next(0, possibleTypes.Count)];
            var level = possibleCombination.Item1;
            var type = possibleCombination.Item2[Random.Shared.Next(0, possibleCombination.Item2.Length)];

            item.Id = type * 1000 + level * 10 + Random.Shared.Next(0, 5);
            // item = GenerateQuality(ref item, mobLevel);

            if (!Collections.ItemType.TryGetValue(item.Id, out var entry))
            {
                FConsole.WriteLine($"[{nameof(ItemGenerator)}] Generated invalid {item.Id} - not found");
                return default;
            }

            item.CurrentDurability = (ushort)Random.Shared.Next(0, entry.AmountLimit);
            item.MaximumDurability = (ushort)Math.Min(entry.AmountLimit, item.CurrentDurability + Random.Shared.Next(0, entry.AmountLimit - item.CurrentDurability));

            item = AddBless(ref item);
            item = AddPlus(ref item);
            item = AddSockets(ref item);

            unsafe
            {
                FConsole.WriteLine($"[{nameof(ItemGenerator)}] Generated {item.Id} - {item.CurrentDurability}/{item.MaximumDurability} - {item.Bless} - {item.Plus} - {item.Gem1} - {item.Gem2} - {item.Enchant} | {Encoding.UTF8.GetString(entry.Name, ItemType.MAX_NAMESIZE).Trim('\0')}");
            }
            return item;
        }


        public static ref ItemComponent AddBless(ref ItemComponent entry)
        {
            var dice = Random.Shared.NextSingle();
            if (dice < 1)
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
    }
}