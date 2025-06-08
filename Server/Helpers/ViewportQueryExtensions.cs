using System.Numerics;
using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using NttECS.ECS;

namespace MagnumOpus.Helpers;

/// <summary>
/// Array-based viewport queries for optimal cache locality and performance.
/// Returns parallel arrays where same index corresponds to same entity across all arrays.
/// </summary>
public static class ViewportQueryExtensions
{
    /// <summary>
    /// Query for entities with a single component type, returning parallel arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ViewportArrayQuery<T> Query<T>(this ref ViewportComponent viewport) where T : struct
    {
        return new ViewportArrayQuery<T>(viewport.EntitiesVisible);
    }

    /// <summary>
    /// Query for entities with two component types, returning parallel arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (NTT[] Entities, T1[] Components1, T2[] Components2) Query<T1, T2>(this ref ViewportComponent viewport) 
        where T1 : struct where T2 : struct
    {
        var entities = new List<NTT>();
        var components1 = new List<T1>();
        var components2 = new List<T2>();

        foreach (var entity in viewport.EntitiesVisible)
        {
            if (!entity.Has<T1>() || !entity.Has<T2>()) continue;
            
            entities.Add(entity);
            components1.Add(entity.Get<T1>());
            components2.Add(entity.Get<T2>());
        }

        return (entities.ToArray(), components1.ToArray(), components2.ToArray());
    }

    /// <summary>
    /// Convenient shortcut for living players.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (NTT[] Entities, PlayerComponent[] Components) Players(this ref ViewportComponent viewport)
    {
        var entities = new List<NTT>();
        var components = new List<PlayerComponent>();

        foreach (var entity in viewport.EntitiesVisible)
        {
            if (!entity.Has<PlayerComponent>() || entity.Has<DeathTagComponent>()) continue;
            
            entities.Add(entity);
            components.Add(entity.Get<PlayerComponent>());
        }

        return (entities.ToArray(), components.ToArray());
    }

    /// <summary>
    /// Convenient shortcut for living monsters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (NTT[] Entities, CqMonsterComponent[] Components) Monsters(this ref ViewportComponent viewport)
    {
        var entities = new List<NTT>();
        var components = new List<CqMonsterComponent>();

        foreach (var entity in viewport.EntitiesVisible)
        {
            if (!entity.Has<CqMonsterComponent>() || entity.Has<DeathTagComponent>()) continue;
            
            entities.Add(entity);
            components.Add(entity.Get<CqMonsterComponent>());
        }

        return (entities.ToArray(), components.ToArray());
    }

    /// <summary>
    /// Quick check if any entities match the component filter without allocating arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any<T>(this ref ViewportComponent viewport) where T : struct
    {
        foreach (var entity in viewport.EntitiesVisible)
        {
            if (entity.Has<T>()) return true;
        }
        return false;
    }
}

/// <summary>
/// Array-based query builder that supports filtering before returning arrays.
/// </summary>
public readonly struct ViewportArrayQuery<T> where T : struct
{
    private readonly HashSet<NTT> _entities;
    private readonly bool _withoutDeath;

    internal ViewportArrayQuery(HashSet<NTT> entities, bool withoutDeath = false)
    {
        _entities = entities;
        _withoutDeath = withoutDeath;
    }

    /// <summary>
    /// Filter out entities with DeathTagComponent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ViewportArrayQuery<T> Without<TWithout>() where TWithout : struct
    {
        if (typeof(TWithout) == typeof(DeathTagComponent))
        {
            return new ViewportArrayQuery<T>(_entities, withoutDeath: true);
        }
        return this; // Only support death filter for now
    }

    /// <summary>
    /// Find the nearest entity to the specified position without allocating arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NTT NearestTo(Vector2 position)
    {
        var nearestEntity = default(NTT);
        var nearestDistanceSq = float.MaxValue;

        foreach (var entity in _entities)
        {
            if (!entity.Has<T>()) continue;
            if (_withoutDeath && entity.Has<DeathTagComponent>()) continue;
            if (!entity.Has<PositionComponent>()) continue;

            ref readonly var entityPos = ref entity.Get<PositionComponent>();
            var distanceSq = Vector2.DistanceSquared(position, entityPos.Position);

            if (distanceSq < nearestDistanceSq)
            {
                nearestDistanceSq = distanceSq;
                nearestEntity = entity;
            }
        }

        return nearestEntity;
    }

    /// <summary>
    /// Get the first entity matching the filters, or default if none found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NTT FirstOrDefault()
    {
        foreach (var entity in _entities)
        {
            if (!entity.Has<T>()) continue;
            if (_withoutDeath && entity.Has<DeathTagComponent>()) continue;
            
            return entity;
        }
        return default;
    }

    /// <summary>
    /// Count entities matching the filters without allocating arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count()
    {
        var count = 0;
        foreach (var entity in _entities)
        {
            if (!entity.Has<T>()) continue;
            if (_withoutDeath && entity.Has<DeathTagComponent>()) continue;
            
            count++;
        }
        return count;
    }

    /// <summary>
    /// Convert to parallel arrays with filtering applied.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out NTT[] entities, out T[] components)
    {
        var entityList = new List<NTT>();
        var componentList = new List<T>();

        foreach (var entity in _entities)
        {
            if (!entity.Has<T>()) continue;
            if (_withoutDeath && entity.Has<DeathTagComponent>()) continue;
            
            entityList.Add(entity);
            componentList.Add(entity.Get<T>());
        }

        entities = entityList.ToArray();
        components = componentList.ToArray();
    }
}