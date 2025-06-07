# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Memories

- Always be fun and casual

## Build and Run Commands

```bash
# Build and run the server
dotnet restore
dotnet build --configuration Release
dotnet run

# Docker deployment
docker build -t magnumopus .
docker run -p 5816:5816 -p 9958:9958 magnumopus
```

## ChangedTick Auto-Tracking

MagnumOpus uses a source generator to automatically manage ChangedTick updates for component properties. This eliminates manual `ChangedTick = NttWorld.Tick` assignments that were easy to forget.

### Converted Components

The following components use auto-tracking (see `/ChangedTickGenerator/` for implementation):

- **PositionComponent**: `Position`, `Direction` properties auto-track
- **ManaComponent**: `Mana`, `MaxMana` properties auto-track  
- **WalkComponent**: `Direction`, `IsRunning` properties auto-track
- **HealthComponent**: Custom properties with network sync + manual ChangedTick

### Code Pattern Changes

**Before (manual tracking):**
```csharp
pos.Position.X = newX;                   // Modify vector component
pos.Position.Y = newY;                   // Modify vector component  
pos.ChangedTick = NttWorld.Tick;         // Manual tracking (easy to forget!)
```

**After (auto-tracking):**
```csharp
pos.Position = new Vector2(newX, newY);  // Set complete value, auto-tracks ChangedTick
```

### Benefits of the New Pattern

1. **Immutable Updates**: Set complete values instead of modifying components
2. **Single ChangedTick Update**: One property assignment = one efficient update
3. **Atomic Operations**: Position changes as complete operation, not partial states
4. **Cannot Forget**: ChangedTick automatically updates when values actually change

### Adding Auto-Tracking to Components

1. Add `[AutoChangedTick]` attribute and `partial` keyword
2. Add `using MagnumOpus.SourceGeneration;`
3. Convert fields to `[Track] private _fieldName;` backing fields
4. Use generated properties in constructors and systems
5. Remove manual `ChangedTick = NttWorld.Tick` assignments

### Network-Synced Components

For components with network sync (like HealthComponent), preserve custom property logic and manually manage ChangedTick within the setter to maintain network behavior.

---

# Comprehensive Codebase Analysis

*Last updated: January 2025*

## Architecture Overview

MagnumOpus is a high-performance MMORPG server implementing a custom Entity Component System (ECS) architecture. The server handles real-time multiplayer gameplay with 60 TPS processing, secure networking, and robust game systems.

### Core Architecture

**Entity Component System (ECS)**:
- **Custom NttECS Library**: Non-generic ECS extracted as reusable library
- **Component-Based Typing**: Entities classified by component presence, not enum values
- **Multi-Threaded Systems**: Parallel processing with configurable thread counts
- **Change Tracking**: Efficient network sync via component ChangedTick timestamps

**Network Architecture**:
- **Dual-Server Design**: Separate login (9958) and game (5816) servers
- **Security-First**: Multiple encryption layers (TQCipher, Blowfish, DH key exchange)
- **Thread-Per-Client**: Dedicated threads for each connection with lock-free packet queues

**Processing Pipeline** (60 TPS):
```
PacketsIn → AI → Movement → Spatial → Viewport → Combat → Damage → Rewards → Cleanup → PacketsOut
```

## Entity Type System Refactoring

The codebase recently underwent major refactoring from enum-based to component-based entity typing:

### Before: EntityType Enum
```csharp
if (entity.Type == EntityType.Player) { }
```

### After: Component Duck Typing
```csharp
public static bool IsPlayer(this NTT ntt) => ntt.Has<NetworkComponent>() || ntt.Has<PlayerComponent>();
public static bool IsMonster(this NTT ntt) => ntt.Has<CqMonsterComponent>();
public static bool IsNpc(this NTT ntt) => ntt.Has<NpcComponent>();
public static bool IsItem(this NTT ntt) => ntt.Has<ItemComponent>();
public static bool IsTrap(this NTT ntt) => ntt.Has<TrapComponent>();
```

### ID Generation Patterns
**Segmented ID Ranges** for type identification:
- NPCs: 0-399,999
- Monsters: 400,000-799,999  
- Traps: 800,000-899,999
- Players: 1,000,000-1,099,999
- Items: 2,000,000-2,999,999
- Others: 3,000,000-3,999,999 (legacy, marked for review)

## Key Game Systems

### Combat System
Multi-stage pipeline: Attack initiation → Target resolution → Damage calculation → Death handling → Experience rewards

### AI Implementation
**Goal-Oriented Action Planning (GOAP)**:
- **BasicAI**: Target acquisition and action planning
- **GuardAI**: Patrol behavior with return-to-position
- **BoidAI**: Flocking behavior for group movement

### Network Synchronization
- **Change Tracking**: Only sync modified components
- **Viewport-Based**: Spatial culling for efficient updates
- **Property-Level**: Granular synchronization per component field

### Economy & Items
- **DAT File Integration**: Legacy Conquer Online format support
- **Dynamic Shops**: Database-driven merchant system
- **Item Durability**: Wear and repair mechanics
- **Drop System**: Timed floor items with spatial management

## Performance & Threading

### Thread Safety
- **Lock-Free Queues**: ConcurrentQueue for packet processing
- **Spatial Hash Locking**: ReaderWriterLockSlim for entity queries
- **Component Immutability**: Value-type components reduce sharing issues

### Memory Management
- **Struct Components**: Reduced GC pressure
- **Span<T> Usage**: Zero-copy packet processing
- **Object Pooling**: Planned for high-frequency allocations

### Spatial Partitioning
Hash-based system with 10x10 cell grid for efficient viewport queries and collision detection.

## Database Integration

**Hybrid Data Strategy**:
- **SQLite**: Structured game data via Entity Framework
- **DAT Files**: Legacy binary formats for items/monsters
- **In-Memory Collections**: O(1) lookups with Dictionary<K,V>

## Security Features

- **Multi-Layer Encryption**: TQCipher (auth) + Blowfish (game)
- **Diffie-Hellman**: Session key establishment
- **Packet Validation**: Size and structure verification
- **Thread Isolation**: Per-client buffers prevent race conditions

## Areas for Future Development

### Technical Improvements
1. **Source Generators**: Complete ChangedTick automation
2. **Event System**: Decouple systems with event-driven architecture
3. **Hot-Reload**: Runtime configuration changes
4. **Metrics**: Enhanced observability and performance monitoring

### Architecture Evolution
1. **Distributed Design**: Multi-server scalability
2. **Plugin Architecture**: Modular game logic
3. **Database Migrations**: Schema versioning
4. **Admin Tools**: In-game administrative interface

### Legacy Migration
1. **DAT File Replacement**: Move to modern formats
2. **Code Modernization**: Consistent patterns throughout
3. **Configuration Centralization**: Unified settings management

## Development Guidelines

### Component Design
- Use `struct` types with `[Component]` attribute
- Implement auto-tracking where possible
- Keep components focused and lightweight
- Prefer immutable updates over field modification

### System Implementation
- Inherit from `NttSystem<T1, T2, ...>` 
- Specify appropriate thread counts
- Handle component absence gracefully
- Use spatial queries for performance

### Network Protocol
- Add `[PacketHandler]` attribute to static methods
- Use `Memory<byte>` for zero-copy processing
- Validate packet structure and size
- Encrypt sensitive data appropriately

### Threading Best Practices
- Use concurrent collections for shared state
- Prefer lock-free algorithms where possible
- Isolate per-client resources
- Design for multi-core scalability

This analysis reflects the current state of the codebase and provides guidance for future development. The architecture demonstrates solid game server engineering with room for continued evolution.