# MagnumOpus TODO List

## Completed

### ECS Performance Improvements (DONE)
- [x] **Viewport Query Optimization**: Replaced iterator pattern with parallel array returns for better performance
- [x] **Multi-Component Queries**: Added Query<T1, T2>() methods returning tuple of component arrays
- [x] **Query Filtering**: Implemented Without() and InRange() filtering for array-based queries
- [x] **BoidSystem Migration**: Updated to use new high-performance array-based API
- [x] **SwapList Implementation**: Fixed Vector<T> issues and added AsSpan(), GetEnumerator() support

### Documentation (DONE)
- [x] **Enum Translations**: Fixed corrupted Chinese characters in GuildRequest, NpcSort enums
- [x] **AssociateMode Research**: Validated friend/enemy packet operations
- [x] **MapFlags Research**: Documented packet 1110 bitwise flags from ElitePVPers

### Build System (DONE)
- [x] **SwapList Integration**: Fixed CollectionsMarshal.AsSpan issues and foreach compatibility
- [x] **System Compilation**: Resolved all ECS component access patterns

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
- [ ] Fix GOAP garbage collection issues
- [ ] **Performance Validation**: Benchmark SwapList vs List in production workloads

### Architecture Improvements
- [ ] **Event System**: Decouple systems with event-driven architecture
- [ ] **Hot-Reload**: Runtime configuration changes
- [ ] **Metrics**: Enhanced observability and performance monitoring

### Architecture Evolution
- [ ] **Admin Tools**: In-game administrative interface

### Legacy Migration
- [ ] **DAT File Replacement**: Move to modern formats
- [ ] **Code Modernization**: Consistent patterns throughout
- [ ] **Configuration Centralization**: Unified settings management

## Performance Tasks

- [ ] Profile equipment system performance under load
- [ ] Optimize inventory helper methods
- [ ] Review spatial hash update frequency

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

Answer: https://cooldown.dev/topic/524-generating-mobs/
if (generator.Grid >= 1)
{
  maxNpc = (int)((generator.BoundCx / generator.Grid) * (generator.BoundCy / generator.Grid));
  if (maxNpc < 0)
  {
    maxNpc = 1;
  }
}
Grid is the `maxnpc` field.


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

## Game Server Optimization (IN PROGRESS)

### Completed
- [x] **ConnectionManager**: Created with ArrayPool buffer pooling and connection limits
- [x] **SocketAsyncEventArgs Planning**: Researched IOCP-based approach for better scalability

### Current Task
- [ ] **SocketAsyncEventArgs Implementation**: Replace async/await with callback-based IOCP networking
  - Update ConnectionManager to pool SocketAsyncEventArgs objects
  - Rewrite GameServer to use event callbacks instead of async methods
  - Eliminate ref local issues across await boundaries
  - Maintain compatibility with existing ECS packet processing
  - Target: Handle 10,000+ concurrent connections efficiently

### Benefits of SocketAsyncEventArgs Approach
- **Performance**: Uses I/O Completion Ports for maximum efficiency
- **Scalability**: No thread-per-connection or async state machine overhead
- **Compatibility**: Works perfectly with ref locals and ECS components
- **Memory**: Object pooling reduces GC pressure
- **Simplicity**: Callback-based approach simpler than async/await for this use case

## Documentation Tasks

- [ ] Document all packet structures
- [ ] Create system interaction diagrams
- [ ] Write performance tuning guide
- [ ] Document all cq_action types
- [ ] Create deployment guide