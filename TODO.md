# MagnumOpus TODO List

## Planned Features

### ChangedTick Auto-Tracking System
**Status**: Not Implemented (Planned)

A source generator to automatically manage ChangedTick updates for component properties, eliminating manual `ChangedTick = NttWorld.Tick` assignments.

#### Implementation Plan
1. Create `/ChangedTickGenerator/` source generator project
2. Add `[AutoChangedTick]` attribute support
3. Implement `[Track]` backing field generation
4. Convert components to use auto-generated properties
5. Remove manual ChangedTick assignments

#### Target Components
- **PositionComponent**: Auto-track `Position`, `Direction`
- **ManaComponent**: Auto-track `Mana`, `MaxMana`
- **WalkComponent**: Auto-track `Direction`, `IsRunning`
- **HealthComponent**: Preserve custom logic for network sync

#### Benefits
- Immutable update patterns
- Atomic operations
- Impossible to forget ChangedTick updates
- Better performance through single updates

## Testing Tasks

- [ ] **Test Bow + Arrow Equip change to Non Bow weapon change**
  - Verify that when a player has a bow equipped with arrows in the left weapon slot
  - And they equip a non-bow weapon (sword, blade, etc.) to the right weapon slot
  - The arrows are automatically unequipped and moved back to inventory
  - Network packets are sent correctly for both weapon equip and arrow unequip
  - Inventory space validation works (if inventory full, prevent weapon change)
  - Logging messages appear when auto-unequipping arrows

## Development Tasks

### Immediate
- [ ] Add more system cleanup following established pattern
- [ ] Implement additional equipment validation rules
- [ ] Add unit tests for equipment system
- [ ] Fix GOAP garbage collection issues

### Architecture Improvements
- [ ] **Source Generators**: Complete ChangedTick automation
- [ ] **Event System**: Decouple systems with event-driven architecture
- [ ] **Hot-Reload**: Runtime configuration changes
- [ ] **Metrics**: Enhanced observability and performance monitoring

### Architecture Evolution
- [ ] **Distributed Design**: Multi-server scalability
- [ ] **Plugin Architecture**: Modular game logic
- [ ] **Database Migrations**: Schema versioning
- [ ] **Admin Tools**: In-game administrative interface

### Legacy Migration
- [ ] **DAT File Replacement**: Move to modern formats
- [ ] **Code Modernization**: Consistent patterns throughout
- [ ] **Configuration Centralization**: Unified settings management

## Performance Tasks

- [ ] Profile equipment system performance under load
- [ ] Optimize inventory helper methods
- [ ] Review spatial hash update frequency
- [ ] Implement object pooling for high-frequency allocations

## ECS Architecture Refactoring

### Critical Components to Refactor

1. **NetworkComponent** (CRITICAL)
   - Remove Socket, Crypto, Dictionary, ConcurrentQueue
   - Extract to NetworkService system
   - Keep only entity ID and connection state

2. **InventoryComponent** (CRITICAL)
   - Replace `Memory<NTT> Items = new NTT[40]`
   - Separate money/CPs into different components
   - Consider sparse storage for items

3. **EquipmentComponent** (HIGH)
   - Replace Dictionary with fixed struct fields
   - Improve cache locality
   - Remove complex access patterns

4. **ViewportComponent** (HIGH)
   - Move HashSets to ViewportSystem
   - Keep only viewport dimensions in component
   - Use spatial queries instead of storing collections

5. **BrainComponent** (HIGH)
   - Extract GOAP planning to AISystem
   - Keep only state and goal in component
   - Remove mutable collections

6. **SpellBookComponent** (MEDIUM-HIGH)
   - Add ChangedTick support
   - Consider sparse array instead of Dictionary
   - Implement proper network sync

7. **TargetCollectionComponent** (MEDIUM-HIGH)
   - Move to system-local temporary data
   - Not suitable as component data
   - Add size limits

8. **CqTaskComponent** (MEDIUM)
   - Convert `int[]` to value type
   - Use fixed buffer or Span<int>
   - Remove heap allocation

## Research Questions

### CQ_GENERATOR
- How does TQ handle generator timers?
- Are generators controlled by cq_actions?
- Why do Titan/Ganoderma spawns not work correctly?
- Should Guard spawns really have max_npc = 1?

### CQ_MONSTERTYPE
- What do AI Type and STC Type fields control?
- Document all unknown columns

### Equipment System
- Research optimal packet ordering for equipment updates
- Why must MsgItem be sent before inventory removal?

### Map Flags
/// <summary>
///     Disable recording the map position into the database.
/// </summary>
public bool IsRecordDisable()
{
    return Type.HasFlag(MapTypeFlag.RecordDisable);
}

Maps have an attribute for "reborn_map" which is the ID of the map you will respawn on if you log in or revive. 
Eg, Metzone is 1212 "GlobeQuest10" aka Adventure Zone 10, but the reborn map is 1219, which is GlobeExit, the Bird Island style looking map. 
You can do checks when reviving or logging in that the current map has Record Disabled and if it is, have them appear in the Reborn_Map instead. 

## Future Optimizations

### Memory Management
- Implement Span<T> more extensively
- Add object pooling for packets
- Reduce allocations in hot paths
- Consider arena allocators for temporary data

### Threading
- Move to work-stealing thread pool
- Implement better load balancing
- Consider async/await for I/O operations
- Optimize lock contention points

### Networking
- Implement packet batching
- Add compression for large updates
- Consider UDP for position updates
- Implement delta compression

## Documentation Tasks

- [ ] Document all packet structures
- [ ] Create system interaction diagrams
- [ ] Write performance tuning guide
- [ ] Document all cq_action types
- [ ] Create deployment guide