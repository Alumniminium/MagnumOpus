using System.Numerics;
using MagnumOpus.ECS;
using Packets.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct InputComponent
    {
        public readonly int EntityId;
        public Vector2 MovementAxis;
        public Vector2 MouseDir;
        public PlayerInput ButtonStates;
        public bool DidBoostLastFrame;

        public InputComponent(int entityId, Vector2 movement, Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
        {
            EntityId = entityId;
            MovementAxis = movement;
            MouseDir = mousePos;
            ButtonStates = buttonState;
            DidBoostLastFrame = false;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}