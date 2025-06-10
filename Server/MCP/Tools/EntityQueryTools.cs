using System.ComponentModel;
using System.Reflection;
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
}