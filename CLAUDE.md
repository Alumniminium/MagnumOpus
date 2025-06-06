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

[... rest of the existing content remains unchanged ...]