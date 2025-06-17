using System.ComponentModel;
using System.Reflection;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using ModelContextProtocol.Server;
using NttECS.ECS;

namespace MagnumOpus.MCP.Tools;

[McpServerToolType]
public static class EntityQueryTools
{
    private static readonly Dictionary<string, Type> ComponentTypeCache = BuildComponentTypeCache();

    private static Dictionary<string, Type> BuildComponentTypeCache()
    {
        var cache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(PositionComponent).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<ComponentAttribute>() != null)
            {
                cache[type.Name] = type;
                // Also add without "Component" suffix for convenience
                if (type.Name.EndsWith("Component"))
                    cache[type.Name.Replace("Component", "")] = type;
            }
        }

        return cache;
    }

    private static string GetEntityType(NTT ntt)
    {
        if (ntt.IsPlayer()) return "player";
        if (ntt.IsMonster(false)) return "monster";
        if (ntt.IsGuard()) return "guard";
        if (ntt.IsNpc()) return "npc";
        if (ntt.IsItem()) return "item";
        if (ntt.IsTrap()) return "trap";
        if (ntt.IsSpawner()) return "spawner";
        return "unknown";
    }

    private static List<(string Name, Type Type)> GetComponentList(NTT ntt)
    {
        var components = new List<(string, Type)>();

        // Cache the specific Has<T>() method with one generic parameter
        var hasMethod = typeof(NTT).GetMethods()
            .Where(m => m.Name == "Has" &&
                       m.IsGenericMethodDefinition &&
                       m.GetGenericArguments().Length == 1 &&
                       m.GetParameters().Length == 0)
            .Single();

        foreach (var kvp in ComponentTypeCache)
        {
            var genericHasMethod = hasMethod.MakeGenericMethod(kvp.Value);
            if ((bool)genericHasMethod.Invoke(ntt, null)!)
            {
                components.Add((kvp.Value.Name, kvp.Value));
            }
        }

        return components;
    }

    [McpServerTool, Description("Get detailed information about a specific entity by ID")]
    public static object? GetEntity(int entityId)
    {
        lock (NttWorld.NTTs)
        {
            if (!NttWorld.EntityExists(entityId))
                return new { error = "Entity not found", entityId };

            ref var ntt = ref NttWorld.GetEntity(entityId);
            var components = GetComponentList(ntt);

            var result = new Dictionary<string, object>
            {
                ["entityId"] = ntt.Id,
                ["type"] = GetEntityType(ntt),
                ["exists"] = true,
                ["componentCount"] = components.Count,
                ["components"] = components.Select(c => c.Name).ToArray()
            };

            // Add position if available
            if (ntt.Has<PositionComponent>())
            {
                ref readonly var pos = ref ntt.Get<PositionComponent>();
                result["position"] = new
                {
                    x = pos.Position.X,
                    y = pos.Position.Y,
                    map = pos.Map,
                    direction = pos.Direction.ToString(),
                    changedTick = pos.ChangedTick
                };
            }

            // Add name if available
            if (ntt.Has<NameTagComponent>())
            {
                ref readonly var name = ref ntt.Get<NameTagComponent>();
                result["name"] = name.Name;
            }

            // Add health if available
            if (ntt.Has<HealthComponent>())
            {
                ref readonly var health = ref ntt.Get<HealthComponent>();
                result["health"] = new
                {
                    current = health.Health,
                    max = health.MaxHealth,
                    percentage = (float)health.Health / health.MaxHealth * 100
                };
            }

            // Add level if available
            if (ntt.Has<LevelComponent>())
            {
                ref readonly var level = ref ntt.Get<LevelComponent>();
                result["level"] = level.Level;
            }

            return result;
        }
    }

    [McpServerTool, Description("Get entities by type (player, monster, npc, item, trap, spawner)")]
    public static object GetEntitiesByType(string entityType, int? mapId = null, int limit = 100, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return new { error = "Entity type cannot be empty" };

        entityType = entityType.ToLower();
        var validTypes = new[] { "player", "monster", "guard", "npc", "item", "trap", "spawner", "all" };

        if (!validTypes.Contains(entityType))
            return new { error = $"Invalid entity type. Valid types: {string.Join(", ", validTypes)}" };

        lock (NttWorld.NTTs)
        {
            var query = NttWorld.NTTs.Values.AsEnumerable();

            // Filter by type
            if (entityType != "all")
            {
                query = entityType switch
                {
                    "player" => query.Where(e => e.IsPlayer()),
                    "monster" => query.Where(e => e.IsMonster(false)),
                    "guard" => query.Where(e => e.IsGuard()),
                    "npc" => query.Where(e => e.IsNpc()),
                    "item" => query.Where(e => e.IsItem()),
                    "trap" => query.Where(e => e.IsTrap()),
                    "spawner" => query.Where(e => e.IsSpawner()),
                    _ => query
                };
            }

            // Filter by map if specified
            if (mapId.HasValue)
            {
                query = query.Where(e =>
                    e.Has<PositionComponent>() &&
                    e.Get<PositionComponent>().Map == mapId.Value);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(offset)
                .Take(limit)
                .Select(ntt =>
                {
                    var entity = new Dictionary<string, object>
                    {
                        ["id"] = ntt.Id,
                        ["type"] = GetEntityType(ntt)
                    };

                    if (ntt.Has<NameTagComponent>())
                        entity["name"] = ntt.Get<NameTagComponent>().Name;

                    if (ntt.Has<PositionComponent>())
                    {
                        ref readonly var pos = ref ntt.Get<PositionComponent>();
                        entity["position"] = new { x = pos.Position.X, y = pos.Position.Y, map = pos.Map };
                    }

                    if (ntt.Has<LevelComponent>())
                        entity["level"] = ntt.Get<LevelComponent>().Level;

                    return entity;
                })
                .ToList();

            return new
            {
                type = entityType,
                mapId,
                totalCount,
                returnedCount = entities.Count,
                offset,
                limit,
                entities
            };
        }
    }

    [McpServerTool, Description("Find all entities that have a specific component")]
    public static object GetEntitiesByComponent(string componentName, int limit = 100, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            return new { error = "Component name cannot be empty" };

        if (!ComponentTypeCache.TryGetValue(componentName, out var componentType))
            return new
            {
                error = "Component type not found",
                hint = "Use GetComponentTypes to see available components"
            };

        lock (NttWorld.NTTs)
        {
            // Get the specific Has<T>() method with one generic parameter
            var hasMethod = typeof(NTT).GetMethods()
                .Where(m => m.Name == "Has" &&
                           m.IsGenericMethodDefinition &&
                           m.GetGenericArguments().Length == 1 &&
                           m.GetParameters().Length == 0)
                .Single()
                .MakeGenericMethod(componentType);

            var query = NttWorld.NTTs.Values
                .Where(ntt => (bool)hasMethod.Invoke(ntt, null)!);

            var totalCount = query.Count();
            var entities = query
                .Skip(offset)
                .Take(limit)
                .Select(ntt => new
                {
                    id = ntt.Id,
                    type = GetEntityType(ntt),
                    name = ntt.Has<NameTagComponent>() ? ntt.Get<NameTagComponent>().Name : null
                })
                .ToList();

            return new
            {
                componentName = componentType.Name,
                totalCount,
                returnedCount = entities.Count,
                offset,
                limit,
                entities
            };
        }
    }

    [McpServerTool, Description("Get list of all components attached to an entity")]
    public static object GetEntityComponents(int entityId)
    {
        lock (NttWorld.NTTs)
        {
            if (!NttWorld.EntityExists(entityId))
                return new { error = "Entity not found", entityId };

            ref var ntt = ref NttWorld.GetEntity(entityId);
            var components = GetComponentList(ntt);

            return new
            {
                entityId,
                type = GetEntityType(ntt),
                componentCount = components.Count,
                components = components.Select(c => new
                {
                    name = c.Name,
                    type = c.Type.Name,
                    hasChangedTick = c.Type.GetField("ChangedTick") != null
                }).ToArray()
            };
        }
    }

    [McpServerTool, Description("Get common stats for an entity (health, mana, level, position, etc)")]
    public static object GetEntityStats(int entityId)
    {
        lock (NttWorld.NTTs)
        {
            if (!NttWorld.EntityExists(entityId))
                return new { error = "Entity not found", entityId };

            ref var ntt = ref NttWorld.GetEntity(entityId);
            var stats = new Dictionary<string, object>
            {
                ["entityId"] = entityId,
                ["type"] = GetEntityType(ntt),
                ["isAlive"] = ntt.IsAlive(),
                ["worldTick"] = NttWorld.Tick
            };

            // Basic info
            if (ntt.Has<NameTagComponent>())
                stats["name"] = ntt.Get<NameTagComponent>().Name;

            // Position
            if (ntt.Has<PositionComponent>())
            {
                ref readonly var pos = ref ntt.Get<PositionComponent>();
                stats["position"] = new
                {
                    x = pos.Position.X,
                    y = pos.Position.Y,
                    map = pos.Map,
                    direction = pos.Direction.ToString()
                };
            }

            // Health
            if (ntt.Has<HealthComponent>())
            {
                ref readonly var health = ref ntt.Get<HealthComponent>();
                stats["health"] = new
                {
                    current = health.Health,
                    max = health.MaxHealth,
                    percentage = Math.Round((float)health.Health / health.MaxHealth * 100, 1)
                };
            }

            // Mana
            if (ntt.Has<ManaComponent>())
            {
                ref readonly var mana = ref ntt.Get<ManaComponent>();
                stats["mana"] = new
                {
                    current = mana.Mana,
                    max = mana.MaxMana,
                    percentage = Math.Round((float)mana.Mana / mana.MaxMana * 100, 1)
                };
            }

            // Level & Experience
            if (ntt.Has<LevelComponent>())
            {
                ref readonly var level = ref ntt.Get<LevelComponent>();
                stats["level"] = level.Level;
                stats["experience"] = level.Experience;
            }

            // Combat stats
            if (ntt.Has<CombatComponent>())
            {
                ref readonly var combat = ref ntt.Get<CombatComponent>();
                stats["combat"] = new
                {
                    minAttack = combat.MinAttack,
                    maxAttack = combat.MaxAttack,
                    defense = combat.Defense,
                    magicAttack = combat.MagicAttack,
                    magicDefense = combat.MagicResist,
                    dodge = combat.Dodge,
                };
            }

            return stats;
        }
    }

    [McpServerTool, Description("Get list of all available component types")]
    public static object GetComponentTypes()
    {
        return new
        {
            totalCount = ComponentTypeCache.Count,
            components = ComponentTypeCache
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new
                {
                    name = kvp.Value.Name,
                    saveEnabled = kvp.Value.GetCustomAttribute<ComponentAttribute>()?.SaveEnabled ?? false
                })
                .DistinctBy(c => c.name)
                .ToArray()
        };
    }

    private static NTT? FindCurrentPlayer()
    {
        lock (NttWorld.NTTs)
        {
            return NttWorld.NTTs.Values.FirstOrDefault(e => e.IsPlayer());
        }
    }

    [McpServerTool, Description("Get comprehensive player data including position, stats, inventory summary")]
    public static object GetPlayerInfo(int? playerId = null)
    {
        lock (NttWorld.NTTs)
        {
            NTT player;
            if (playerId.HasValue)
            {
                if (!NttWorld.EntityExists(playerId.Value))
                    return new { error = "Player not found", playerId };
                player = NttWorld.GetEntity(playerId.Value);
                if (!player.IsPlayer())
                    return new { error = "Entity is not a player", playerId };
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                player = foundPlayer.Value;
            }

            var result = new Dictionary<string, object>
            {
                ["entityId"] = player.Id,
                ["type"] = "player",
                ["worldTick"] = NttWorld.Tick
            };

            if (player.Has<NameTagComponent>())
                result["name"] = player.Get<NameTagComponent>().Name;

            if (player.Has<PositionComponent>())
            {
                ref readonly var pos = ref player.Get<PositionComponent>();
                result["position"] = new
                {
                    x = pos.Position.X,
                    y = pos.Position.Y,
                    map = pos.Map,
                    direction = pos.Direction.ToString()
                };
            }

            if (player.Has<HealthComponent>())
            {
                ref readonly var health = ref player.Get<HealthComponent>();
                result["health"] = new
                {
                    current = health.Health,
                    max = health.MaxHealth,
                    percentage = Math.Round((float)health.Health / health.MaxHealth * 100, 1)
                };
            }

            if (player.Has<ManaComponent>())
            {
                ref readonly var mana = ref player.Get<ManaComponent>();
                result["mana"] = new
                {
                    current = mana.Mana,
                    max = mana.MaxMana,
                    percentage = Math.Round((float)mana.Mana / mana.MaxMana * 100, 1)
                };
            }

            if (player.Has<LevelComponent>())
            {
                ref readonly var level = ref player.Get<LevelComponent>();
                result["level"] = level.Level;
                result["experience"] = level.Experience;
            }

            if (player.Has<InventoryComponent>())
            {
                ref readonly var inv = ref player.Get<InventoryComponent>();
                var itemCount = 0;
                var emptySlots = 0;
                for (int i = 0; i < inv.Items.Length; i++)
                {
                    if (inv.Items.Span[i].Id != 0)
                        itemCount++;
                    else
                        emptySlots++;
                }
                result["inventory"] = new
                {
                    money = inv.Money,
                    cps = inv.CPs,
                    itemCount,
                    emptySlots,
                    totalSlots = inv.Items.Length
                };
            }

            return result;
        }
    }

    [McpServerTool, Description("Get all entities visible to a specific entity")]
    public static object GetViewport(int? entityId = null, bool includeDetails = false)
    {
        lock (NttWorld.NTTs)
        {
            NTT entity;
            if (entityId.HasValue)
            {
                if (!NttWorld.EntityExists(entityId.Value))
                    return new { error = "Entity not found", entityId };
                entity = NttWorld.GetEntity(entityId.Value);
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                entity = foundPlayer.Value;
            }

            if (!entity.Has<ViewportComponent>())
                return new { error = "Entity does not have viewport", entityId = entity.Id };

            ref readonly var viewport = ref entity.Get<ViewportComponent>();
            var visibleEntities = new List<object>();

            foreach (var visibleEntity in viewport.EntitiesVisible)
            {
                var entityInfo = new Dictionary<string, object>
                {
                    ["id"] = visibleEntity.Id,
                    ["type"] = GetEntityType(visibleEntity)
                };

                if (visibleEntity.Has<NameTagComponent>())
                    entityInfo["name"] = visibleEntity.Get<NameTagComponent>().Name;

                if (visibleEntity.Has<PositionComponent>())
                {
                    ref readonly var pos = ref visibleEntity.Get<PositionComponent>();
                    entityInfo["position"] = new { x = pos.Position.X, y = pos.Position.Y };

                    if (entity.Has<PositionComponent>())
                    {
                        ref readonly var myPos = ref entity.Get<PositionComponent>();
                        var distance = Math.Sqrt(Math.Pow(pos.Position.X - myPos.Position.X, 2) + Math.Pow(pos.Position.Y - myPos.Position.Y, 2));
                        entityInfo["distance"] = Math.Round(distance, 1);
                    }
                }

                if (includeDetails)
                {
                    if (visibleEntity.Has<LevelComponent>())
                        entityInfo["level"] = visibleEntity.Get<LevelComponent>().Level;

                    if (visibleEntity.Has<HealthComponent>())
                    {
                        ref readonly var health = ref visibleEntity.Get<HealthComponent>();
                        entityInfo["health"] = new
                        {
                            current = health.Health,
                            max = health.MaxHealth,
                            percentage = Math.Round((float)health.Health / health.MaxHealth * 100, 1)
                        };
                    }
                }

                visibleEntities.Add(entityInfo);
            }

            return new
            {
                entityId = entity.Id,
                worldTick = NttWorld.Tick,
                visibleCount = visibleEntities.Count,
                entities = visibleEntities
            };
        }
    }

    [McpServerTool, Description("Get detailed inventory and money information")]
    public static object GetInventory(int? entityId = null)
    {
        lock (NttWorld.NTTs)
        {
            NTT entity;
            if (entityId.HasValue)
            {
                if (!NttWorld.EntityExists(entityId.Value))
                    return new { error = "Entity not found", entityId };
                entity = NttWorld.GetEntity(entityId.Value);
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                entity = foundPlayer.Value;
            }

            if (!entity.Has<InventoryComponent>())
                return new { error = "Entity does not have inventory", entityId = entity.Id };

            ref readonly var inv = ref entity.Get<InventoryComponent>();
            var items = new List<object>();

            for (int i = 0; i < inv.Items.Length; i++)
            {
                var item = inv.Items.Span[i];
                if (item.Id != 0)
                {
                    var itemInfo = new Dictionary<string, object>
                    {
                        ["slot"] = i,
                        ["entityId"] = item.Id,
                        ["type"] = GetEntityType(item)
                    };

                    if (item.Has<NameTagComponent>())
                        itemInfo["name"] = item.Get<NameTagComponent>().Name;

                    if (item.Has<ItemComponent>())
                    {
                        ref readonly var itemComp = ref item.Get<ItemComponent>();
                        itemInfo["amount"] = itemComp.StackAmount;
                        itemInfo["itemType"] = ItemHelper.GetItemType(itemComp.StackAmount);
                    }

                    items.Add(itemInfo);
                }
            }

            var emptySlots = inv.Items.Length - items.Count;

            return new
            {
                entityId = entity.Id,
                money = inv.Money,
                cps = inv.CPs,
                totalSlots = inv.Items.Length,
                usedSlots = items.Count,
                emptySlots,
                items
            };
        }
    }

    [McpServerTool, Description("Find all entities targeting a specific entity")]
    public static object GetThreats(int? entityId = null)
    {
        lock (NttWorld.NTTs)
        {
            NTT targetEntity;
            if (entityId.HasValue)
            {
                if (!NttWorld.EntityExists(entityId.Value))
                    return new { error = "Entity not found", entityId };
                targetEntity = NttWorld.GetEntity(entityId.Value);
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                targetEntity = foundPlayer.Value;
            }

            var threats = new List<object>();

            foreach (var entity in NttWorld.NTTs.Values)
            {
                if (entity.Has<BrainComponent>())
                {
                    ref readonly var brain = ref entity.Get<BrainComponent>();
                    if (brain.Target.Id == targetEntity.Id)
                    {
                        var threatInfo = new Dictionary<string, object>
                        {
                            ["id"] = entity.Id,
                            ["type"] = GetEntityType(entity),
                            ["brainState"] = brain.State.ToString()
                        };

                        if (entity.Has<NameTagComponent>())
                            threatInfo["name"] = entity.Get<NameTagComponent>().Name;

                        if (entity.Has<PositionComponent>() && targetEntity.Has<PositionComponent>())
                        {
                            ref readonly var pos = ref entity.Get<PositionComponent>();
                            ref readonly var targetPos = ref targetEntity.Get<PositionComponent>();
                            var distance = Math.Sqrt(Math.Pow(pos.Position.X - targetPos.Position.X, 2) + Math.Pow(pos.Position.Y - targetPos.Position.Y, 2));
                            threatInfo["distance"] = Math.Round(distance, 1);
                            threatInfo["position"] = new { x = pos.Position.X, y = pos.Position.Y };
                        }

                        if (entity.Has<LevelComponent>())
                            threatInfo["level"] = entity.Get<LevelComponent>().Level;

                        threats.Add(threatInfo);
                    }
                }
            }

            return new
            {
                targetEntityId = targetEntity.Id,
                threatCount = threats.Count,
                threats
            };
        }
    }

    [McpServerTool, Description("Find closest entity by type/name with filters")]
    public static object FindNearestEntity(int? fromEntityId = null, string? entityType = null, string? nameFilter = null, float maxDistance = 100f, int limit = 10)
    {
        lock (NttWorld.NTTs)
        {
            NTT fromEntity;
            if (fromEntityId.HasValue)
            {
                if (!NttWorld.EntityExists(fromEntityId.Value))
                    return new { error = "Source entity not found", fromEntityId };
                fromEntity = NttWorld.GetEntity(fromEntityId.Value);
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                fromEntity = foundPlayer.Value;
            }

            if (!fromEntity.Has<PositionComponent>())
                return new { error = "Source entity has no position", fromEntityId = fromEntity.Id };

            ref readonly var fromPos = ref fromEntity.Get<PositionComponent>();
            var candidates = new List<(NTT entity, float distance)>();

            foreach (var entity in NttWorld.NTTs.Values)
            {
                if (entity.Id == fromEntity.Id || !entity.Has<PositionComponent>())
                    continue;

                ref readonly var pos = ref entity.Get<PositionComponent>();
                if (pos.Map != fromPos.Map)
                    continue;

                var distance = (float)Math.Sqrt(Math.Pow(pos.Position.X - fromPos.Position.X, 2) + Math.Pow(pos.Position.Y - fromPos.Position.Y, 2));
                if (distance > maxDistance)
                    continue;

                if (!string.IsNullOrEmpty(entityType))
                {
                    var actualType = GetEntityType(entity).ToLower();
                    if (actualType != entityType.ToLower())
                        continue;
                }

                if (!string.IsNullOrEmpty(nameFilter) && entity.Has<NameTagComponent>())
                {
                    var name = entity.Get<NameTagComponent>().Name;
                    if (!name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                candidates.Add((entity, distance));
            }

            var results = candidates
                .OrderBy(c => c.distance)
                .Take(limit)
                .Select(c =>
                {
                    var entity = c.entity;
                    var entityInfo = new Dictionary<string, object>
                    {
                        ["id"] = entity.Id,
                        ["type"] = GetEntityType(entity),
                        ["distance"] = Math.Round(c.distance, 1)
                    };

                    if (entity.Has<NameTagComponent>())
                        entityInfo["name"] = entity.Get<NameTagComponent>().Name;

                    if (entity.Has<PositionComponent>())
                    {
                        ref readonly var pos = ref entity.Get<PositionComponent>();
                        entityInfo["position"] = new { x = pos.Position.X, y = pos.Position.Y };
                    }

                    if (entity.Has<LevelComponent>())
                        entityInfo["level"] = entity.Get<LevelComponent>().Level;

                    return entityInfo;
                })
                .ToList();

            return new
            {
                fromEntityId = fromEntity.Id,
                searchRadius = maxDistance,
                entityType,
                nameFilter,
                totalFound = results.Count,
                entities = results
            };
        }
    }

    [McpServerTool, Description("Calculate total sell value of inventory items")]
    public static object CalculateSellValue(int? entityId = null)
    {
        lock (NttWorld.NTTs)
        {
            NTT entity;
            if (entityId.HasValue)
            {
                if (!NttWorld.EntityExists(entityId.Value))
                    return new { error = "Entity not found", entityId };
                entity = NttWorld.GetEntity(entityId.Value);
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                entity = foundPlayer.Value;
            }

            if (!entity.Has<InventoryComponent>())
                return new { error = "Entity does not have inventory", entityId = entity.Id };

            ref readonly var inv = ref entity.Get<InventoryComponent>();
            uint totalValue = 0;
            var itemBreakdown = new List<object>();

            for (int i = 0; i < inv.Items.Length; i++)
            {
                var item = inv.Items.Span[i];
                if (item.Id != 0 && item.Has<ItemComponent>())
                {
                    ref readonly var itemComp = ref item.Get<ItemComponent>();
                    // Note: This is a simplified calculation. In a real implementation,
                    // you'd need to look up the actual sell price from item data tables
                    uint estimatedValue = (uint)(itemComp.StackAmount * 10); // Using StackAmount instead of Amount
                    totalValue += estimatedValue;

                    var itemInfo = new Dictionary<string, object>
                    {
                        ["slot"] = i,
                        ["itemId"] = itemComp.Id, // Using Id instead of ItemType
                        ["stackAmount"] = itemComp.StackAmount,
                        ["estimatedSellValue"] = estimatedValue
                    };

                    if (item.Has<NameTagComponent>())
                        itemInfo["name"] = item.Get<NameTagComponent>().Name;

                    itemBreakdown.Add(itemInfo);
                }
            }

            return new
            {
                entityId = entity.Id,
                currentMoney = inv.Money,
                totalSellValue = totalValue,
                projectedTotal = inv.Money + totalValue,
                itemCount = itemBreakdown.Count,
                items = itemBreakdown
            };
        }
    }

    [McpServerTool, Description("Get spatial hash cell information for positions")]
    public static object GetSpatialInfo(int? x = null, int? y = null, int? mapId = null, int? entityId = null)
    {
        lock (NttWorld.NTTs)
        {
            Vector2 position;
            int map;

            if (entityId.HasValue)
            {
                if (!NttWorld.EntityExists(entityId.Value))
                    return new { error = "Entity not found", entityId };
                var entity = NttWorld.GetEntity(entityId.Value);
                if (!entity.Has<PositionComponent>())
                    return new { error = "Entity has no position", entityId };
                ref readonly var pos = ref entity.Get<PositionComponent>();
                position = pos.Position;
                map = pos.Map;
            }
            else if (x.HasValue && y.HasValue && mapId.HasValue)
            {
                position = new Vector2(x.Value, y.Value);
                map = mapId.Value;
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found and no position specified" };
                var player = foundPlayer.Value;
                if (!player.Has<PositionComponent>())
                    return new { error = "Player has no position" };
                ref readonly var pos = ref player.Get<PositionComponent>();
                position = pos.Position;
                map = pos.Map;
            }

            // Calculate spatial hash cell (using same logic as SpatialHash class)
            const int cellSize = 10; // Default cell size from SpatialHash
            var cellX = (int)(position.X / cellSize);
            var cellY = (int)(position.Y / cellSize);
            var hash = (cellX * 73856093) ^ (cellY * 19349663);

            // Find entities in the same cell
            var entitiesInCell = new List<object>();
            foreach (var entity in NttWorld.NTTs.Values)
            {
                if (!entity.Has<PositionComponent>())
                    continue;
                ref readonly var entityPos = ref entity.Get<PositionComponent>();
                if (entityPos.Map != map)
                    continue;

                var entityCellX = (int)(entityPos.Position.X / cellSize);
                var entityCellY = (int)(entityPos.Position.Y / cellSize);
                if (entityCellX == cellX && entityCellY == cellY)
                {
                    var entityInfo = new Dictionary<string, object>
                    {
                        ["id"] = entity.Id,
                        ["type"] = GetEntityType(entity),
                        ["position"] = new { x = entityPos.Position.X, y = entityPos.Position.Y }
                    };

                    if (entity.Has<NameTagComponent>())
                        entityInfo["name"] = entity.Get<NameTagComponent>().Name;

                    entitiesInCell.Add(entityInfo);
                }
            }

            return new
            {
                position = new { x = position.X, y = position.Y },
                map,
                spatialCell = new
                {
                    x = cellX,
                    y = cellY,
                    hash,
                    cellSize
                },
                entitiesInCell = entitiesInCell.Count,
                entities = entitiesInCell
            };
        }
    }

    [McpServerTool, Description("Get server performance metrics")]
    public static object GetServerStats()
    {
        lock (NttWorld.NTTs)
        {
            var entityCounts = new Dictionary<string, int>();
            string[] validTypes = ["player", "monster", "guard", "npc", "item", "trap", "spawner"];

            foreach (var type in validTypes)
            {
                entityCounts[type] = type switch
                {
                    "player" => NttWorld.NTTs.Values.Count(e => e.IsPlayer()),
                    "monster" => NttWorld.NTTs.Values.Count(e => e.IsMonster(false)),
                    "guard" => NttWorld.NTTs.Values.Count(e => e.IsGuard()),
                    "npc" => NttWorld.NTTs.Values.Count(e => e.IsNpc()),
                    "item" => NttWorld.NTTs.Values.Count(e => e.IsItem()),
                    "trap" => NttWorld.NTTs.Values.Count(e => e.IsTrap()),
                    "spawner" => NttWorld.NTTs.Values.Count(e => e.IsSpawner()),
                    _ => 0
                };
            }

            return new
            {
                worldTick = NttWorld.Tick,
                totalEntities = NttWorld.NTTs.Count,
                entityBreakdown = entityCounts,
                performance = new
                {
                    // Note: TPS and timing information would need to be exposed
                    // from the game server's main loop. This is placeholder data.
                    note = "Performance metrics would require server instrumentation"
                }
            };
        }
    }

    [McpServerTool, Description("Get performance stats for specific systems")]
    public static object GetSystemStats(string? systemName = null)
    {
        // Note: This would require instrumentation of the ECS systems
        // to track execution times and entity counts per system
        return new
        {
            error = "System performance metrics not yet implemented",
            note = "This would require instrumentation of NttSystem execution",
            systemName
        };
    }

    [McpServerTool, Description("Search entities by name pattern")]
    public static object FindEntityByName(string namePattern, string? entityType = null, int? mapId = null, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return new { error = "Name pattern cannot be empty" };

        lock (NttWorld.NTTs)
        {
            var matches = new List<object>();

            foreach (var entity in NttWorld.NTTs.Values)
            {
                if (!entity.Has<NameTagComponent>())
                    continue;

                var name = entity.Get<NameTagComponent>().Name;
                if (!name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(entityType))
                {
                    var actualType = GetEntityType(entity);
                    if (!string.Equals(actualType, entityType, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (mapId.HasValue && entity.Has<PositionComponent>())
                {
                    ref readonly var pos = ref entity.Get<PositionComponent>();
                    if (pos.Map != mapId.Value)
                        continue;
                }

                var entityInfo = new Dictionary<string, object>
                {
                    ["id"] = entity.Id,
                    ["name"] = name,
                    ["type"] = GetEntityType(entity)
                };

                if (entity.Has<PositionComponent>())
                {
                    ref readonly var pos = ref entity.Get<PositionComponent>();
                    entityInfo["position"] = new { x = pos.Position.X, y = pos.Position.Y, map = pos.Map };
                }

                if (entity.Has<LevelComponent>())
                    entityInfo["level"] = entity.Get<LevelComponent>().Level;

                matches.Add(entityInfo);

                if (matches.Count >= limit)
                    break;
            }

            return new
            {
                namePattern,
                entityType,
                mapId,
                totalFound = matches.Count,
                limit,
                entities = matches
            };
        }
    }

    [McpServerTool, Description("Get available actions/commands for a player")]
    public static object GetAvailableActions(int? playerId = null)
    {
        lock (NttWorld.NTTs)
        {
            NTT player;
            if (playerId.HasValue)
            {
                if (!NttWorld.EntityExists(playerId.Value))
                    return new { error = "Player not found", playerId };
                player = NttWorld.GetEntity(playerId.Value);
                if (!player.IsPlayer())
                    return new { error = "Entity is not a player", playerId };
            }
            else
            {
                var foundPlayer = FindCurrentPlayer();
                if (!foundPlayer.HasValue)
                    return new { error = "No players found in world" };
                player = foundPlayer.Value;
            }

            var actions = new List<string>
            {
                "move", "look", "say", "whisper"
            };

            // Add contextual actions based on components and nearby entities
            if (player.Has<InventoryComponent>())
            {
                actions.AddRange(["drop", "use", "equip", "unequip"]);
            }

            if (player.Has<CombatComponent>())
            {
                actions.AddRange(["attack", "cast"]);
            }

            // Check for nearby interactive entities
            if (player.Has<ViewportComponent>())
            {
                ref readonly var viewport = ref player.Get<ViewportComponent>();
                var hasNearbyNpc = viewport.EntitiesVisible.Any(e => e.IsNpc());
                var hasNearbyItem = viewport.EntitiesVisible.Any(e => e.IsItem());
                var hasNearbyPlayer = viewport.EntitiesVisible.Any(e => e.IsPlayer() && e.Id != player.Id);

                if (hasNearbyNpc)
                    actions.Add("talk");
                if (hasNearbyItem)
                    actions.Add("pickup");
                if (hasNearbyPlayer)
                    actions.AddRange(["trade", "team", "friend"]);
            }

            return new
            {
                playerId = player.Id,
                availableActions = actions.Distinct().OrderBy(a => a).ToArray(),
                note = "This is a basic list. Actual available actions depend on game state and nearby entities."
            };
        }
    }
}