# Tools for MagnumOpus MCP Server

## MCP Tools Implementation Plan

### High Priority Tools (Core Functionality)

- [x] **GetPlayerInfo** - Get comprehensive player data including position, stats, inventory summary
  - Covers: "What's my location/position/map?", "How much money do I have?"
  - Parameters: `playerId` (optional - defaults to current developer)
  - Returns: Position, health, money, basic inventory counts

- [x] **GetViewport** - Get all entities visible to a specific entity
  - Covers: "What can I see?"
  - Parameters: `entityId`, `includeDetails` (bool)
  - Returns: List of visible entities with basic info

- [x] **GetInventory** - Get detailed inventory and money information
  - Covers: "How many items do I have?", "How much money do I have?"
  - Parameters: `entityId`
  - Returns: Detailed item list, money, slot usage

- [x] **GetThreats** - Find all entities targeting a specific entity
  - Covers: "Who is targeting me?"
  - Parameters: `entityId`
  - Returns: List of entities with targeting component pointing to target

- [x] **FindNearestEntity** - Find closest entity by type/name with filters
  - Covers: "Where is the closest monster/player/NPC/item?", "Where is the closest [Monster Name]?"
  - Parameters: `fromEntityId`, `entityType`, `nameFilter`, `maxDistance`, `limit`
  - Returns: Sorted list of nearby entities with distances

### Medium Priority Tools (Enhanced Functionality)

- [x] **CalculateSellValue** - Calculate total sell value of inventory items
  - Covers: "How much money will I have after I sell everything?"
  - Parameters: `entityId`
  - Returns: Total sell value, item breakdown

- [x] **GetSpatialInfo** - Get spatial hash cell information for positions
  - Covers: "Which grid cell am I in?"
  - Parameters: `x`, `y`, `mapId` OR `entityId`
  - Returns: Grid cell coordinates, entities in cell

- [x] **GetServerStats** - Get server performance metrics
  - Covers: "What's the tick time of the server?", "How many entities are in the world?"
  - Parameters: None
  - Returns: Current tick, TPS, entity counts, memory usage

- [x] **GetSystemStats** - Get performance stats for specific systems
  - Covers: "What's the tick time of a certain system?", "How many entities are in a certain system?"
  - Parameters: `systemName`
  - Returns: System execution time, entity count, status

- [x] **FindEntityByName** - Search entities by name pattern
  - Covers: "Where is [NPC Name]?"
  - Parameters: `namePattern`, `entityType`, `mapId`
  - Returns: Matching entities with positions

### Low Priority Tools (Quality of Life)

- [x] **GetAvailableActions** - Get available actions/commands for a player
  - Covers: "What can I do?"
  - Parameters: `playerId`
  - Returns: Available commands, contextual actions

### Tool Chaining Examples

These tools are designed to work together:

- **"Statistics of monster attacking me"**: `GetThreats(playerId)` → `GetEntityStats(attackerId)`
- **"Closest monster to me"**: `GetPlayerInfo()` → `FindNearestEntity(playerId, "monster")`
- **"How much money after selling everything"**: `GetInventory(playerId)` → `CalculateSellValue(playerId)`
