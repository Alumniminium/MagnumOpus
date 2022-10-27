using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class InputSystem : PixelSystem<InputComponent>
    {

        public InputSystem() : base("InputSystem System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref InputComponent c1)
        {
            
        }
    }
}