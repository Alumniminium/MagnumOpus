using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SourceGeneration;

namespace MagnumOpus.Components
{
    /// <summary>
    /// Example component demonstrating the ChangedTick source generator.
    /// 
    /// This component shows how to use [AutoChangedTick] and [Track] attributes
    /// to automatically generate properties that update ChangedTick when values change.
    /// 
    /// BEFORE (manual ChangedTick management):
    /// - You had to remember to set ChangedTick = NttWorld.Tick after every change
    /// - Easy to forget, leading to sync issues
    /// - Repetitive boilerplate code
    /// 
    /// AFTER (auto-generated ChangedTick):
    /// - Just set properties normally: component.Position = newPos;
    /// - ChangedTick automatically updates when values actually change
    /// - No more forgetting to update ChangedTick!
    /// </summary>
    [Component(saveEnabled: true), AutoChangedTick]
    public partial struct ExampleAutoComponent
    {
        // Required: ChangedTick field for tracking changes
        public long ChangedTick = NttWorld.Tick;
        
        // Tracked fields - generator will create properties and backing fields for these
        [Track] private Vector2 _position;               // Generator creates Position property
        [Track] private int _health;                     // Generator creates Health property
        [Track] private Direction _direction;            // Generator creates Direction property
        
        // Regular fields - not tracked, remain as fields
        public Vector2 LastPosition;                     // No change tracking
        public int MaxHealth;                            // No change tracking
        public string? Name;                             // No change tracking

        // Constructor to initialize values using the generated properties
        public ExampleAutoComponent(Vector2 position, int health)
        {
            // Initialize non-tracked fields directly
            LastPosition = position;
            MaxHealth = health;
            Name = null;
            
            // Initialize tracked properties (these will be generated)
            Position = position;          // Will use generated property
            Health = health;              // Will use generated property  
            Direction = Direction.North;  // Will use generated property
        }
    }

    /// <summary>
    /// Advanced example showing custom equality and options.
    /// </summary>
    [Component, AutoChangedTick(TickSource = "NttWorld.Tick", UseEqualityCheck = true)]
    public partial struct AdvancedAutoComponent
    {
        public long ChangedTick = NttWorld.Tick;
        
        // Custom equality method for floating point comparison
        [Track(CustomEqualityMethod = "FloatApproximateEquals")]
        private float _temperature;
        
        // Always update ChangedTick, even if value hasn't changed
        [Track(AlwaysUpdate = true)]
        private string _status = "";
        
        // Custom backing field name
        [Track(BackingFieldName = "_pos")]
        private Vector2 _position;
        
        // Constructor required when struct has field initializers
        public AdvancedAutoComponent()
        {
            Temperature = 20.0f;
            Status = "Active";
            Position = Vector2.Zero;
        }
        
        // Custom equality method - must be static and match signature
        private static bool FloatApproximateEquals(float a, float b)
        {
            return System.Math.Abs(a - b) < 0.001f;
        }
    }
}