# CLAUDE.md

Project guidance for Claude Code when working with MagnumOpus.

## Quick Reference

### Build & Run
```bash
# Development
dotnet restore
dotnet build --configuration Release
dotnet run

# Docker deployment
docker build -t magnumopus .
docker run -p 5816:5816 -p 9958:9958 magnumopus
```

### Ports
- Login Server: 9958
- Game Server: 5816

## Project Overview

MagnumOpus is a high-performance MMORPG server implementing a custom Entity Component System (ECS) architecture. The server handles real-time multiplayer gameplay with 60 TPS processing, secure networking, and robust game systems.

## Architecture

### Entity Component System (ECS)
- **NttECS Library**: Custom non-generic ECS extracted as reusable library
- **Component-Based Typing**: Entities classified by component presence, not enum values
- **Multi-Threaded Systems**: Parallel processing with configurable thread counts (see thread_count in systems)
- **Change Tracking**: Manual `ChangedTick = NttWorld.Tick` for network synchronization

### Entity Identification

Entities are identified by their components (duck typing):

```csharp
// From NttExtension.cs
public static bool IsPlayer(this NTT ntt) => ntt.Has<NetworkComponent>() || ntt.Has<PlayerComponent>();
public static bool IsMonster(this NTT ntt) => ntt.Has<CqMonsterComponent>();
public static bool IsNpc(this NTT ntt) => ntt.Has<NpcComponent>();
public static bool IsItem(this NTT ntt) => ntt.Has<ItemComponent>();
public static bool IsTrap(this NTT ntt) => ntt.Has<TrapComponent>();
```

**ID Ranges** (from IdGenerator.cs):
- NPCs: 0 - 399,999
- Monsters: 400,000 - 799,999
- Traps: 800,000 - 899,999
- Players: 1,000,000 - 1,099,999
- Items: 2,000,000 - 2,999,999
- Others: 3,000,000 - 3,999,999 (legacy, marked for review)

### Processing Pipeline (60 TPS)
```
PacketsIn → AI → Movement → Spatial → Viewport → Combat → Damage → Rewards → Cleanup → PacketsOut
```

Systems are numbered (000-999) for explicit ordering.

### Network Architecture
- **Dual-Server Design**: Separate login and game servers
- **Security**: TQCipher (auth) + Blowfish (game) + Diffie-Hellman key exchange
- **Thread-Per-Client**: Dedicated threads with lock-free packet queues (ConcurrentQueue)
- **Zero-Copy**: Uses `Memory<byte>` and `Span<T>` for packet processing

## Development Guidelines

### Component Design
- Use `struct` types with `[Component]` attribute
- Keep components focused on data, not logic
- Manually update `ChangedTick = NttWorld.Tick` when modifying network-synced data
- Avoid reference types (Dictionary, List, HashSet) in components
- Prefer value types and fixed-size data

### System Implementation
- Inherit from `NttSystem<T1, T2, ...>` with required components
- Set appropriate `thread_count` in constructor
- Handle missing components gracefully (components can be added/removed at runtime)
- Use spatial queries for viewport-based operations
- Number systems (000-999) for processing order

### Network Protocol
- Add `[PacketHandler(PacketType)]` attribute to static handler methods
- Handlers receive `(Client client, Memory<byte> packet)`
- Use `Memory<byte>` for zero-copy processing
- Validate packet structure and size
- Handle encryption/decryption through client.Crypto

### Threading Best Practices
- Use `ConcurrentQueue` for cross-thread communication
- Spatial hash uses `ReaderWriterLockSlim` for thread safety
- Isolate per-client state to avoid contention
- Design systems for parallel execution

## Implementation Notes

### Equipment System
- **Critical**: Send `MsgItem` packet with `SetEquipPosition(5)` BEFORE removing item from inventory
- Auto-unequip arrows when switching from bow to non-bow weapon
- Validate inventory space before equipment changes

### Monster Spawning (cq_generator)
- `born_x/y`: Start position of spawn area
- `born_cx/cy`: Size of spawn area (width/height)
- `max_per_gen`: Monsters spawned per interval
- `rest_secs`: Seconds between spawn intervals
- `max_npc`: Maximum living monsters (skip spawn if reached)
- `npctype`: References cq_monstertype.id

### Monster Types (cq_monstertype)
- `action`: Links to cq_action.id for special behaviors (drops, events)
- Many unknown fields (AI_Type, STC_Type) - legacy from Conquer Online

## Known Issues

### Architecture Violations
These components violate ECS best practices and need refactoring:

1. **NetworkComponent**: Contains Socket, Crypto, acts as service instead of data
2. **InventoryComponent**: Large arrays (40 items), mixed concerns (items + money)
3. **EquipmentComponent**: Dictionary storage breaks cache locality
4. **ViewportComponent**: Mutable HashSets cause allocations
5. **BrainComponent**: Contains GOAP planning logic, not just state

### Technical Debt
- GOAP system causes excessive garbage collection
- Legacy DAT file formats from Conquer Online
- Components with reference types violate data-oriented design
- Some spawn mechanics (Titan, Ganoderma) not working correctly

## Database Integration
- **SQLite**: Main game database via Entity Framework
- **DAT Files**: Legacy binary formats (itemtype.dat, monster.dat, etc.)
- **In-Memory**: Dictionaries for O(1) lookups during runtime

## Personal Notes
- Always be fun and casual
- See TODO.md for planned features and improvements