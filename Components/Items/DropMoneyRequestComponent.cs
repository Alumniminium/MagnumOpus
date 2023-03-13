using MagnumOpus.ECS;
namespace MagnumOpus.Components.Items
{
    [Component]
    public struct RequestDropMoneyComponent
    {
        public int Amount;

        public RequestDropMoneyComponent(int amount) => Amount = amount;
    }
}