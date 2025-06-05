# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

## Architecture Overview

**MagnumOpus** is a high-performance ECS-based game server written in C# (.NET 9) for a Conquer Online-style MMORPG. The architecture prioritizes performance with aggressive inlining, unsafe code blocks, and custom memory management targeting 60 TPS.

### Core Architecture Components

**Dual Server Design:**
- **LoginServer** (Port 9958): Authentication and character selection using TQCipher
- **GameServer** (Port 5816): Gameplay with Diffie-Hellman + Blowfish encryption

**ECS Framework:**
- **NTT**: Lightweight entity struct supporting up to 6 component types per system
- **Components**: Struct-based components in `SparseComponentStorage<T>` for cache efficiency
- **Systems**: 33+ systems processing specific component combinations
- **NttWorld**: Central entity manager handling creation, destruction, and updates

**Data Layer:**
- **SQLite** database (`Squiggly.db`) with Entity Framework Core
- **Hybrid Loading**: Database records + encrypted CLIENT_FILES/*.dat files
- **Static Collections**: In-memory caches for maps, items, monsters, etc.

### Key System Categories

```
Network I/O: PacketsIn/PacketsOut systems
AI: BasicAISystem, GuardAISystem, BoidSystem  
Movement: WalkSystem, JumpSystem, TeleportSystem
Combat: AttackSystem, MagicAttackSystem, DamageSystem
World: ViewportSystem, SpatialHashSystem, PortalSystem
Items: PickupSystem, DropSystem, EquipSystem, ShopSystem
```

### Critical Components

- **NetworkComponent**: Socket management, crypto, packet queues
- **PositionComponent**: World coordinates and map ID
- **ViewportComponent**: Vision/interaction range for message broadcasting
- **BodyComponent**: Visual appearance and equipment synchronization

### Networking Architecture

**Packet System:**
- 40+ strongly-typed packet structs in `/Networking/Packets/`
- Per-packet-type concurrent queues for thread-safe processing
- Viewport-based message distribution using spatial hashing

**Cryptography:**
- Login: TQCipher encryption
- Game: BlowfishCipher with Diffie-Hellman key exchange
- Custom Co2Packet serialization with zero-copy operations

### Database Structure

**Core Tables:**
- `cq_map`: World maps and zones
- `cq_npc`: Static NPCs and behaviors  
- `cq_monstertype`: Monster definitions
- `cq_generator`: Spawn points and rules
- `cq_action/cq_task`: Quest scripting system
- `cq_portal`: Map transitions

**Data Loading:**
1. Decrypt and load CLIENT_FILES/*.dat (items, magic, monsters)
2. Load SQLite database records via Entity Framework
3. Create ECS entities for static world objects
4. Initialize spatial partitioning and caches

### Performance Characteristics

- **60 TPS** target with microsecond timing precision
- **Spatial hashing** for efficient proximity queries
- **Unsafe code** and `Span<T>` for zero-copy operations
- **Lock-free collections** for networking
- **Prometheus metrics** tracking (2.5% CPU budget achieved)

### Development Notes

**Environment Variables:**
- `GAME_PORT` (default: 5816)
- `LOGIN_PORT` (default: 9958)
- `PROMETHEUS_PORT` (default: 1234)
- `PUBLIC_IP` (default: 192.168.0.209)

**Legacy Compatibility:**
- Conquer Online client protocol (circa 2005)
- Custom encryption schemes (TQ/Blowfish/COFAC ciphers)
- Binary DAT file decryption from CLIENT_FILES/

**State Management:**
- Hot state saving to `_STATE_FILES/` directory
- Manual save trigger with 'S' key during debugging
- Component reflection system for dynamic loading

**Known Issues:**
- GOAP AI system needs rewrite
- Equipment sync requires specific packet ordering
- Generator spawn timing follows mysterious original game logic