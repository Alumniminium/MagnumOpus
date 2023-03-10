using MagnumOpus.ECS;
namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropMoneyComponent
    {
        public readonly int Amount;

        public RequestDropMoneyComponent(int amount) => Amount = amount;
    }
}