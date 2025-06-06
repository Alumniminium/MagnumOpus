# ChangedTick Source Generator

This source generator automatically creates property wrappers for component fields that need to track when they change. It eliminates the need to manually set `ChangedTick = NttWorld.Tick` throughout your codebase.

## What Are Source Generators?

Source generators are compile-time code generators that analyze your source code and generate additional C# code automatically. They run during compilation and add new files to your project without modifying existing code.

**Key Benefits:**
- ✅ **Zero runtime overhead** - all work happens at compile time
- ✅ **Type-safe** - generates strongly typed code with compile-time validation
- ✅ **No reflection** - faster than runtime reflection-based solutions
- ✅ **Incremental** - only regenerates when source code changes

## How This Generator Works

The ChangedTick generator looks for:
1. **Components** marked with `[AutoChangedTick]` attribute
2. **Fields** marked with `[Track]` attribute within those components
3. **Generates property wrappers** that automatically update `ChangedTick`

## Usage Examples

### Before (Manual ChangedTick Management)
```csharp
[Component]
public struct PositionComponent(Vector2 position, int map)
{
    public long ChangedTick = NttWorld.Tick;
    public Vector2 Position = position;
    public Vector2 LastPosition = position;
    public Direction Direction;
    public int Map = map;
}

// In systems, you had to remember:
public void Update(ref PositionComponent pos)
{
    pos.Position = newPosition;
    pos.ChangedTick = NttWorld.Tick; // Easy to forget!
    
    pos.Direction = newDirection;
    pos.ChangedTick = NttWorld.Tick; // Repetitive!
}
```

### After (Auto-Generated ChangedTick)
```csharp
[Component, AutoChangedTick]
public partial struct PositionComponent(Vector2 position, int map)
{
    public long ChangedTick = NttWorld.Tick;
    
    [Track] public Vector2 Position = position;     // Auto-tracked
    [Track] public Direction Direction;             // Auto-tracked
    public Vector2 LastPosition = position;         // Not tracked
    public int Map = map;                          // Not tracked
}

// In systems, just set the property:
public void Update(ref PositionComponent pos)
{
    pos.Position = newPosition;    // ChangedTick automatically updated!
    pos.Direction = newDirection;  // ChangedTick automatically updated!
}
```

### Generated Code (What the compiler creates for you)
```csharp
// Auto-generated partial class
public partial struct PositionComponent
{
    // Private backing fields
    private Vector2 _position;
    private Direction _direction;
    
    // Auto-generated properties with change tracking
    public Vector2 Position 
    {
        get => _position;
        set 
        {
            if (!EqualityComparer<Vector2>.Default.Equals(_position, value)) 
            {
                _position = value;
                ChangedTick = NttWorld.Tick;
            }
        }
    }
    
    public Direction Direction 
    {
        get => _direction;
        set 
        {
            if (!EqualityComparer<Direction>.Default.Equals(_direction, value)) 
            {
                _direction = value;
                ChangedTick = NttWorld.Tick;
            }
        }
    }
}
```

## Step-by-Step Usage Guide

### 1. Mark Your Component
Add `[AutoChangedTick]` attribute and make the struct `partial`:
```csharp
[Component, AutoChangedTick]
public partial struct MyComponent  // ← Note: partial keyword required!
{
    public long ChangedTick = NttWorld.Tick;
    // ... your fields
}
```

### 2. Mark Fields to Track
Add `[Track]` attribute to fields that should update ChangedTick:
```csharp
[Track] public Vector2 Position;     // Will become auto-property
[Track] public int Health;           // Will become auto-property
public string Name;                  // Remains a regular field
```

### 3. Initialize Tracked Fields
For tracked fields with initial values, initialize them in the constructor:
```csharp
public partial struct PositionComponent(Vector2 position, int map)
{
    public long ChangedTick = NttWorld.Tick;
    
    [Track] public Vector2 Position;
    public int Map = map;  // Not tracked, direct assignment OK
    
    // Constructor sets initial values for tracked fields
    public PositionComponent(Vector2 position, int map) : this()
    {
        Position = position;  // Uses generated property setter
        Map = map;           // Direct field assignment
    }
}
```

### 4. Build Your Project
The source generator runs automatically during compilation. You'll see the generated code in:
- **Visual Studio**: Solution Explorer → Dependencies → Analyzers → SourceGeneration.ChangedTickGenerator
- **Rider**: External Sources → Generated Files

## Advanced Usage

### Custom Equality Comparison
For complex types, you can customize equality checking:
```csharp
[Track(CustomEqualityMethod = "CustomVectorEquals")]
public CustomVector Position;

private static bool CustomVectorEquals(CustomVector a, CustomVector b)
{
    return Math.Abs(a.X - b.X) < 0.001f && Math.Abs(a.Y - b.Y) < 0.001f;
}
```

### Performance Considerations
The generator creates efficient code:
- **Equality checks prevent unnecessary updates** - ChangedTick only updates when value actually changes
- **Generic EqualityComparer** - uses fastest comparison for each type
- **Inline methods** - properties are marked for aggressive inlining
- **Struct optimization** - preserves struct semantics and performance

## Integration with Existing Code

### Gradual Migration
You can migrate components one at a time:
```csharp
// Keep existing manual implementation
public struct OldComponent 
{
    public long ChangedTick;
    public int Value;
    
    public void SetValue(int newValue) 
    {
        Value = newValue;
        ChangedTick = NttWorld.Tick;
    }
}

// New auto-generated implementation  
[Component, AutoChangedTick]
public partial struct NewComponent
{
    public long ChangedTick = NttWorld.Tick;
    [Track] public int Value;
}
```

### Working with Existing Properties
If you already have properties with custom logic, the generator won't override them:
```csharp
[Component, AutoChangedTick]
public partial struct MixedComponent
{
    public long ChangedTick = NttWorld.Tick;
    
    [Track] public int SimpleValue;  // Auto-generated property
    
    // Custom property - generator skips this
    public int ComplexValue 
    {
        get => _complexValue;
        set 
        {
            if (value < 0) value = 0;  // Custom validation
            if (_complexValue != value) 
            {
                _complexValue = value;
                ChangedTick = NttWorld.Tick;
                // Custom side effects...
            }
        }
    }
    private int _complexValue;
}
```

## Troubleshooting

### Common Issues

**"Partial keyword missing"**
```csharp
// ❌ Wrong - missing partial
[AutoChangedTick]
public struct MyComponent { }

// ✅ Correct - has partial
[AutoChangedTick] 
public partial struct MyComponent { }
```

**"Field not becoming property"**
```csharp
// ❌ Wrong - missing [Track] attribute
public Vector2 Position;

// ✅ Correct - has [Track] attribute
[Track] public Vector2 Position;
```

**"Generated code not appearing"**
- Clean and rebuild solution
- Check that the generator is properly referenced in .csproj
- Verify component has both `[AutoChangedTick]` and `partial` keywords

### Debugging Generated Code

To see what code is being generated:
1. **Visual Studio**: View → Other Windows → Error List, then look for SourceGeneration warnings
2. **Command Line**: `dotnet build -v detailed` shows generation logs
3. **Output Files**: Check obj/Debug/net9.0/generated/ folder

## Performance Impact

### Compile Time
- ⏱️ **Fast incremental generation** - only regenerates changed files
- ⏱️ **Parallel processing** - analyzes multiple components simultaneously  
- ⏱️ **Efficient caching** - remembers previous analysis results

### Runtime Performance
- 🚀 **Zero overhead** - no runtime reflection or dynamic code
- 🚀 **Optimized equality checks** - prevents unnecessary ChangedTick updates
- 🚀 **Inlined properties** - JIT compiler optimizes generated code
- 🚀 **Struct semantics preserved** - maintains value type performance

## Status

✅ **Fully Working** - The source generator builds and integrates successfully with the main project
✅ **External Architecture** - Generator is properly isolated outside the main project directory
✅ **Component Detection** - Generator correctly identifies components marked with `[AutoChangedTick]`
✅ **Code Generation** - Properties with change tracking are generated successfully
✅ **Build Integration** - Project builds cleanly with no errors or warnings
✅ **Proper Isolation** - Generator project is standalone and can be reused across projects

## Architecture

The source generator follows proper separation of concerns:

```
MagnumOpus/
├── ChangedTickGenerator/           # External source generator project
│   ├── SourceGeneration.Generator.csproj
│   ├── ChangedTickGenerator.cs
│   ├── ComponentAnalyzer.cs
│   ├── ComponentTemplate.cs
│   ├── CodeGenHelpers.cs
│   └── README.md
└── Server/                        # Main game server project
    ├── MagnumOpus.csproj          # References external generator
    ├── Components/
    │   └── ExampleAutoComponent.cs
    └── ... (game code)
```

## Known Issues Fixed

1. **Duplicate Assembly Attributes** - Fixed by isolating generator project properly
2. **Missing Using Statements** - Fixed by adding proper using directives to generated code
3. **Math Namespace Issues** - Fixed by using `System.Math.Abs` in example components
4. **Project Reference Conflicts** - Fixed by using separate generator project with proper isolation
5. **Duplicate Compile Items** - Fixed by disabling default compile items in generator project
6. **External Architecture** - Moved generator outside main project for proper separation

## Future Enhancements

Potential improvements for this generator:
- **Change event callbacks** - notify when specific fields change
- **Dirty flags** - track which specific fields changed
- **Batch updates** - group multiple changes into single tick update
- **Conditional tracking** - enable/disable tracking at runtime
- **Change history** - maintain log of recent changes

---

*This source generator is designed to make MagnumOpus development faster and less error-prone while maintaining the high-performance ECS architecture.*