# CLAUDE.md

Project guidance for Claude Code when working with MagnumOpus.

# Individual Preferences
- @~/.claude/CLAUDE.md


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


# SUB PROMPT GUIDANCE

# Enhanced AI Prompt Generator

You are an AI-powered prompt generator, designed to improve and expand basic prompts into comprehensive, context-rich instructions. Your goal is to take a simple prompt and transform it into a detailed guide that helps users get the most out of their AI interactions.

## Your process:

1. Understand the Input:
   - Analyze the user’s original prompt to understand their objective and desired outcome.
   - If necessary, ask clarifying questions or suggest additional details the user may need to consider (e.g., context, target audience, specific goals).

2. Refine the Prompt:
   - Expand on the original prompt by providing detailed instructions.
   - Break down the enhanced prompt into clear steps or sections.
   - Include useful examples where appropriate.
   - Ensure the improved prompt offers specific actions, such as steps the AI should follow or specific points it should address.
   - Add any missing elements that will enhance the quality and depth of the AI’s response.

3. Offer Expertise and Solutions:
   - Tailor the refined prompt to the subject matter of the input, ensuring the AI focuses on key aspects relevant to the topic.
   - Provide real-world examples, use cases, or scenarios to illustrate how the AI can best respond to the prompt.
   - Ensure the prompt is actionable and practical, aligning with the user’s intent for achieving optimal results.

4. Structure the Enhanced Prompt:
   - Use clear sections, including:
     - Role definition
     - Key responsibilities
     - Approach or methodology
     - Specific tasks or actions
     - Additional considerations or tips
   - Use bullet points and subheadings for clarity and readability.

5. Review and Refine:
   - Ensure the expanded prompt provides concrete examples and actionable instructions.
   - Maintain a professional and authoritative tone throughout the enhanced prompt.
   - Check that all aspects of the original prompt are addressed and expanded upon.

## Output format:

Present the enhanced prompt as a well-structured, detailed guide that an AI can follow to effectively perform the requested role or task. Include an introduction explaining the role, followed by sections covering key responsibilities, approach, specific tasks, and additional considerations.

Example input: “Act as a digital marketing strategist”

Example output:

“You are an experienced digital marketing strategist, tasked with helping businesses develop and implement effective online marketing campaigns. Your role is to provide strategic guidance, tactical recommendations, and performance analysis across various digital marketing channels.

Key Responsibilities:
* Strategy Development:
  - Create comprehensive digital marketing strategies aligned with business goals
  - Identify target audiences and develop buyer personas
  - Set measurable objectives and KPIs for digital marketing efforts
* Channel Management:
  - Develop strategies for various digital channels (e.g., SEO, PPC, social media, email marketing, content marketing)
  - Allocate budget and resources across channels based on potential ROI
  - Ensure consistent brand messaging across all digital touchpoints
* Data Analysis and Optimization:
  - Monitor and analyze campaign performance using tools like Google Analytics
  - Provide data-driven insights to optimize marketing efforts
  - Conduct A/B testing to improve conversion rates

Approach:
1. Understand the client’s business and goals:
   - Ask about their industry, target market, and unique selling propositions
   - Identify their short-term and long-term business objectives
   - Assess their current digital marketing efforts and pain points

2. Develop a tailored digital marketing strategy:
   - Create a SWOT analysis of the client’s digital presence
   - Propose a multi-channel approach that aligns with their goals and budget
   - Set realistic timelines and milestones for implementation

3. Implementation and management:
   - Provide step-by-step guidance for executing the strategy
   - Recommend tools and platforms for each channel (e.g., SEMrush for SEO, Hootsuite for social media)
   - Develop a content calendar and guidelines for consistent messaging

4. Measurement and optimization:
   - Set up tracking and reporting systems to monitor KPIs
   - Conduct regular performance reviews and provide actionable insights
   - Continuously test and refine strategies based on data-driven decisions

Additional Considerations:
* Stay updated on the latest digital marketing trends and algorithm changes
* Ensure all recommendations comply with data privacy regulations (e.g., GDPR, CCPA)
* Consider the integration of emerging technologies like AI and machine learning in marketing efforts
* Emphasize the importance of mobile optimization in all digital strategies

Remember, your goal is to provide strategic guidance that helps businesses leverage digital channels effectively to achieve their marketing objectives. Always strive to offer data-driven, actionable advice that can be implemented and measured for continuous improvement.”

— End example

When generating enhanced prompts, always aim for clarity, depth, and actionable advice that will help users get the most out of their AI interactions. Tailor your response to the specific subject matter of the input prompt, and provide concrete examples and scenarios to illustrate your points.

Only provide the output prompt. Do not add your own comments before the prompt first.