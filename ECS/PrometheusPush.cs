using Prometheus;

namespace MagnumOpus.ECS
{
    public static class PrometheusPush
    {
        public static readonly Gauge NTTCount = Metrics.CreateGauge("MAGNUMOPUS_ENTITIES", "Counter for Entities");
        public static readonly Counter TickCount = Metrics.CreateCounter("MAGNUMOPUS_TICKS", "Counter for Ticks");
        public static readonly Histogram TickTime = Metrics.CreateHistogram("MAGNUMOPUS_TICK", "Time taken for a tick");
        public static readonly Counter EntityChangedCount = Metrics.CreateCounter("MAGNUMOPUS_ENTITY_CHANGED", "Counter for Entity Changed");


        public static readonly Counter NTTCreations = Metrics.CreateCounter("MO_NTT_CREATION_COUNT", "Amount of times an NTT was created");
        public static readonly Counter NTTDestroys = Metrics.CreateCounter("MO_NTT_DELETION_COUNT", "Amount of times an NTT was destroyed");
        public static readonly Counter NTTChanges = Metrics.CreateCounter("MO_NTT_CHANGE_COUNT",  "Amount of times an NTT was changed");
        public static readonly Counter LoginCount = Metrics.CreateCounter("MO_LOGIN_COUNT",  "Amount of times a player logged in");
        public static readonly Counter WalkCount = Metrics.CreateCounter("MO_WALK_COUNT", "Amount of times a player walked");
        public static readonly Counter JumpCount = Metrics.CreateCounter("MO_JUMP_COUNT", "Amount of times a player jumped");
        public static readonly Counter JumpDistance = Metrics.CreateCounter("MO_JUMP_DISTANCE", "Amount of distance a player jumped");


        #region Economy Metrics
        public static readonly Counter ShopPurchases = Metrics.CreateCounter("MO_DROPS_SHOP_PURCHASE", "Amount of Shop Purchases");
        public static readonly Counter ShopSales = Metrics.CreateCounter("MO_DROPS_SHOP_SELL", "Amount of Shop Sales");
        public static readonly Counter ShopIncome = Metrics.CreateCounter("MO_SHOP_INCOME", "Amount of Money earned by the Shop");
        public static readonly Counter ShopExpenses = Metrics.CreateCounter("MO_SHOP_EXPENSES", "Amount of Money spent by the Shop");

        public static readonly Counter ServerIncome = Metrics.CreateCounter("MO_SERVER_INCOME", "Amount of Money earned by the Server");
        public static readonly Counter ServerExpenses = Metrics.CreateCounter("MO_SERVER_EXPENSES", "Amount of Money spent by the Server");
# endregion
        #region Drop Metrics
        public static readonly Counter MeteorDropsCount = Metrics.CreateCounter("MO_DROPS_METEOR", "Amount of Meteors dropped");
        public static readonly Counter DragonballDropsCount = Metrics.CreateCounter("MO_DROPS_DRAGONBALL", "Amount of Dragonballs dropped");
        public static readonly Counter MoneyCount = Metrics.CreateCounter("MO_DROPS_MONEY", "Amount of Money dropped");
        public static readonly Counter MoneyTotal = Metrics.CreateCounter("MO_DROPS_MONEY_TOTAL", "Total Amount of Money dropped");
        public static readonly Counter HealthPotionsCount = Metrics.CreateCounter("MO_DROPS_HEALTH_POTIONS", "Amount of Health Potions dropped");
        public static readonly Counter ManaPotionsCount = Metrics.CreateCounter("MO_DROPS_MANA_POTIONS", "Amount of Mana Potions dropped");

        #region Quality Drops
        public static readonly Counter RefinedWeapons = Metrics.CreateCounter("MO_DROPS_REFINED_WEAPONS", "Amount of Refined Weapons dropped");
        public static readonly Counter RefinedArmors = Metrics.CreateCounter("MO_DROPS_REFINED_ARMORS", "Amount of Refined Armors dropped");
        public static readonly Counter RefinedShields = Metrics.CreateCounter("MO_DROPS_REFINED_SHIELDS", "Amount of Refined Shields dropped");
        public static readonly Counter RefinedHelmets = Metrics.CreateCounter("MO_DROPS_REFINED_HELMETS", "Amount of Refined Helmets dropped");
        public static readonly Counter RefinedBoots = Metrics.CreateCounter("MO_DROPS_REFINED_BOOTS", "Amount of Refined Boots dropped");
        public static readonly Counter RefinedRings = Metrics.CreateCounter("MO_DROPS_REFINED_RINGS", "Amount of Refined Rings dropped");
        public static readonly Counter RefinedNecklaces = Metrics.CreateCounter("MO_DROPS_REFINED_NECKLACES", "Amount of Refined Necklaces dropped");

        public static readonly Counter UniqueWeapons = Metrics.CreateCounter("MO_DROPS_UNIQUE_WEAPONS", "Amount of Unique Weapons dropped");
        public static readonly Counter UniqueArmors = Metrics.CreateCounter("MO_DROPS_UNIQUE_ARMORS", "Amount of Unique Armors dropped");
        public static readonly Counter UniqueShields = Metrics.CreateCounter("MO_DROPS_UNIQUE_SHIELDS", "Amount of Unique Shields dropped");
        public static readonly Counter UniqueHelmets = Metrics.CreateCounter("MO_DROPS_UNIQUE_HELMETS", "Amount of Unique Helmets dropped");
        public static readonly Counter UniqueBoots = Metrics.CreateCounter("MO_DROPS_UNIQUE_BOOTS", "Amount of Unique Boots dropped");
        public static readonly Counter UniqueRings = Metrics.CreateCounter("MO_DROPS_UNIQUE_RINGS", "Amount of Unique Rings dropped");
        public static readonly Counter UniqueNecklaces = Metrics.CreateCounter("MO_DROPS_UNIQUE_NECKLACES", "Amount of Unique Necklaces dropped");

        public static readonly Counter EliteWeapons = Metrics.CreateCounter("MO_DROPS_ELITE_WEAPONS", "Amount of Elite Weapons dropped");
        public static readonly Counter EliteArmors = Metrics.CreateCounter("MO_DROPS_ELITE_ARMORS", "Amount of Elite Armors dropped");
        public static readonly Counter EliteShields = Metrics.CreateCounter("MO_DROPS_ELITE_SHIELDS", "Amount of Elite Shields dropped");
        public static readonly Counter EliteHelmets = Metrics.CreateCounter("MO_DROPS_ELITE_HELMETS", "Amount of Elite Helmets dropped");
        public static readonly Counter EliteBoots = Metrics.CreateCounter("MO_DROPS_ELITE_BOOTS", "Amount of Elite Boots dropped");
        public static readonly Counter EliteRings = Metrics.CreateCounter("MO_DROPS_ELITE_RINGS", "Amount of Elite Rings dropped");
        public static readonly Counter EliteNecklaces = Metrics.CreateCounter("MO_DROPS_ELITE_NECKLACES", "Amount of Elite Necklaces dropped");

        public static readonly Counter SuperWeapons = Metrics.CreateCounter("MO_DROPS_SUPER_WEAPONS", "Amount of Super Weapons dropped");
        public static readonly Counter SuperArmors = Metrics.CreateCounter("MO_DROPS_SUPER_ARMORS", "Amount of Super Armors dropped");
        public static readonly Counter SuperShields = Metrics.CreateCounter("MO_DROPS_SUPER_SHIELDS", "Amount of Super Shields dropped");
        public static readonly Counter SuperHelmets = Metrics.CreateCounter("MO_DROPS_SUPER_HELMETS", "Amount of Super Helmets dropped");
        public static readonly Counter SuperBoots = Metrics.CreateCounter("MO_DROPS_SUPER_BOOTS", "Amount of Super Boots dropped");
        public static readonly Counter SuperRings = Metrics.CreateCounter("MO_DROPS_SUPER_RINGS", "Amount of Super Rings dropped");
        public static readonly Counter SuperNecklaces = Metrics.CreateCounter("MO_DROPS_SUPER_NECKLACES", "Amount of Super Necklaces dropped");

        public static readonly Counter PlusWeapons = Metrics.CreateCounter("MO_DROPS_PLUS_WEAPONS", "Amount of Plus Weapons dropped");
        public static readonly Counter PlusArmors = Metrics.CreateCounter("MO_DROPS_PLUS_ARMORS", "Amount of Plus Armors dropped");
        public static readonly Counter PlusShields = Metrics.CreateCounter("MO_DROPS_PLUS_SHIELDS", "Amount of Plus Shields dropped");
        public static readonly Counter PlusHelmets = Metrics.CreateCounter("MO_DROPS_PLUS_HELMETS", "Amount of Plus Helmets dropped");
        public static readonly Counter PlusBoots = Metrics.CreateCounter("MO_DROPS_PLUS_BOOTS", "Amount of Plus Boots dropped");
        public static readonly Counter PlusRings = Metrics.CreateCounter("MO_DROPS_PLUS_RINGS", "Amount of Plus Rings dropped");
        public static readonly Counter PlusNecklaces = Metrics.CreateCounter("MO_DROPS_PLUS_NECKLACES", "Amount of Plus Necklaces dropped");

        #endregion

        #endregion
    }
}