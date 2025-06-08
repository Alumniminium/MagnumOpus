# Query Filters Implementation Plan

## Current State Analysis

### Existing Infrastructure
1. **NttQuery exists but is unused** - The codebase has a complete NttQuery system with enumerators supporting 1-6 component types, but it's never used in the actual systems.

2. **Manual ChangedTick tracking** - Components like `PositionComponent` and `HealthComponent` have `ChangedTick` fields that are manually updated (or automatically via `NetworkHelper.UpdateSyncedField`).

3. **System-based filtering** - Systems currently use `MatchesFilter()` overrides with manual `Has<T>()` checks (like `002-BasicAISystem.cs:15`).

## Phase 1: Core Query Filter Infrastructure

### 1.1 Filter Interfaces and Base Types
```csharp
// New filter marker interfaces
public interface IQueryFilter<T> where T : struct { }
public interface IWithout<T> : IQueryFilter<T> where T : struct { }
public interface IChanged<T> : IQueryFilter<T> where T : struct { }
public interface IAdded<T> : IQueryFilter<T> where T : struct { }
public interface IOr<T1, T2> : IQueryFilter<T1> where T1 : struct where T2 : struct { }

// Concrete filter types
public struct Without<T> : IWithout<T> where T : struct { }
public struct Changed<T> : IChanged<T> where T : struct { }
public struct Added<T> : IAdded<T> where T : struct { }
public struct Or<T1, T2> : IOr<T1, T2> where T1 : struct where T2 : struct { }
```

### 1.2 Enhanced Query Enumerators
Extend existing `QueryEnumerator<T>` structs to support filter chains:
```csharp
public struct QueryEnumerator<T> where T : struct
{
    // Add filter support
    public QueryEnumerator<T> Without<TExclude>() where TExclude : struct 
        => new FilteredQueryEnumerator<T, Without<TExclude>>(this);
    
    public QueryEnumerator<T> Changed() 
        => new FilteredQueryEnumerator<T, Changed<T>>(this);
    
    public QueryEnumerator<T> Added() 
        => new FilteredQueryEnumerator<T, Added<T>>(this);
}
```

### 1.3 Filtered Query Enumerator
```csharp
public struct FilteredQueryEnumerator<T, TFilter> : IEnumerator<NTT>
    where T : struct 
    where TFilter : struct, IQueryFilter<T>
{
    private QueryEnumerator<T> _baseEnumerator;
    private TFilter _filter;
    
    public bool MoveNext()
    {
        while (_baseEnumerator.MoveNext())
        {
            var current = _baseEnumerator.Current;
            if (ApplyFilter(current, _filter))
                return true;
        }
        return false;
    }
    
    private static bool ApplyFilter(NTT ntt, TFilter filter) => filter switch
    {
        Without<T> => !ntt.Has<T>(),
        Changed<T> => IsChanged<T>(ntt),
        Added<T> => IsAdded<T>(ntt), 
        Or<T1, T2> => ntt.Has<T1>() || ntt.Has<T2>(),
        _ => true
    };
}
```

## Phase 2: Change Detection Infrastructure

### 2.1 Change Tracking Enhancement
Enhance `SparseComponentStorage<T>` to track component lifecycle:
```csharp
public static class SparseComponentStorage<T> where T : struct
{
    private static readonly Dictionary<int, long> ChangeTicks = [];
    private static readonly Dictionary<int, long> AddTicks = [];
    
    public static bool IsChanged(NTT ntt, long sinceTime)
    {
        return ChangeTicks.TryGetValue(ntt.Id, out var tick) && tick >= sinceTime;
    }
    
    public static bool IsAdded(NTT ntt, long sinceTime)
    {
        return AddTicks.TryGetValue(ntt.Id, out var tick) && tick >= sinceTime;
    }
}
```

### 2.2 Automatic ChangedTick Updates
Modify component setters to automatically update change tracking:
```csharp
public static void AddFor(in NTT ntt, ref T c)
{
    lockObj.EnterWriteLock();
    try
    {
        var isNew = !Components.ContainsKey(ntt.Id);
        Components[ntt.Id] = c;
        
        ChangeTicks[ntt.Id] = NttWorld.Tick;
        if (isNew)
            AddTicks[ntt.Id] = NttWorld.Tick;
    }
    finally
    {
        lockObj.ExitWriteLock();
    }
}
```

## Phase 3: Query API Integration

### 3.1 Fluent Query Builder
```csharp
public static class NttQuery
{
    public static QueryBuilder<T> Query<T>() where T : struct 
        => new QueryBuilder<T>();
}

public struct QueryBuilder<T> where T : struct
{
    public QueryBuilder<T> Without<TExclude>() where TExclude : struct 
        => new FilteredQueryBuilder<T, Without<TExclude>>();
    
    public QueryBuilder<T> Changed() 
        => new FilteredQueryBuilder<T, Changed<T>>();
    
    public QueryBuilder<T> Added() 
        => new FilteredQueryBuilder<T, Added<T>>();
    
    public QueryEnumerator<T> Build() => new(NttWorld.NTTs);
}
```

### 3.2 System Integration
Enable systems to use the enhanced query API:
```csharp
// In BasicAISystem.cs
public override void Update()
{
    foreach (var ntt in NttQuery.Query<PositionComponent>()
        .Without<DeathTagComponent>()
        .Changed())
    {
        // Process only living entities with position changes
        ref var pos = ref ntt.Get<PositionComponent>();
        // ... AI logic
    }
}
```

## Phase 4: Performance Optimization

### 4.1 Query Result Caching
```csharp
public static class QueryCache
{
    private static readonly Dictionary<Type, HashSet<NTT>> CachedResults = [];
    
    public static void InvalidateFor<T>(NTT ntt) where T : struct
    {
        // Invalidate cached queries that include component T
    }
}
```

### 4.2 Change Detection Optimization
- Implement bitfield-based change tracking for better cache locality
- Add bulk change detection for systems processing multiple entities
- Optimize lock contention with per-component-type locks

## Phase 5: Migration and Testing

### 5.1 Gradual Migration
1. Start by making `NttQuery` available alongside existing system filtering
2. Convert one system at a time to use the new query API
3. Add performance benchmarks to ensure no regressions

### 5.2 Testing Strategy
```csharp
[Test]
public void QueryFilter_Without_ExcludesEntities()
{
    var ntt1 = NttWorld.CreateEntity(1);
    var ntt2 = NttWorld.CreateEntity(2);
    
    ntt1.Set<HealthComponent>();
    ntt2.Set<HealthComponent>();
    ntt2.Set<DeathTagComponent>();
    
    var results = NttQuery.Query<HealthComponent>()
        .Without<DeathTagComponent>()
        .ToList();
    
    Assert.Contains(ntt1, results);
    Assert.DoesNotContain(ntt2, results);
}
```

## Implementation Benefits

1. **Developer Experience**: Eliminates manual filtering in systems (`!ntt.Has<DeathTagComponent>()`)
2. **Performance**: Query result caching and optimized change detection
3. **Safety**: Compile-time query validation and automatic change tracking
4. **Compatibility**: Existing systems continue working while new ones can adopt enhanced queries
5. **Bevy-Like API**: Familiar patterns for developers coming from Bevy ECS

## Target Usage Examples

### Before (Current System)
```csharp
public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
{
    protected override bool MatchesFilter(in NTT ntt) => 
        ntt.IsMonster() && !ntt.Has<DeathTagComponent>() && !ntt.Has<GuardPositionComponent>();
        
    public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref BrainComponent brn)
    {
        // Manual filtering already done in MatchesFilter
        // ... AI logic
    }
}
```

### After (With Query Filters)
```csharp
public sealed class BasicAISystem : NttSystem
{
    public override void Update()
    {
        foreach (var ntt in NttQuery.Query<PositionComponent>()
            .With<ViewportComponent>()
            .With<BrainComponent>()
            .Without<DeathTagComponent>()
            .Without<GuardPositionComponent>()
            .Where(ntt => ntt.IsMonster()))
        {
            ref var pos = ref ntt.Get<PositionComponent>();
            ref var vwp = ref ntt.Get<ViewportComponent>();
            ref var brn = ref ntt.Get<BrainComponent>();
            // ... AI logic
        }
    }
}
```

This plan provides a comprehensive path to implement Bevy-style query filters while maintaining NttECS's explicit control and C# idiomatic approach.