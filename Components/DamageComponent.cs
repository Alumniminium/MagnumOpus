using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct DamageComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Attacker;
        public int Damage;

        public DamageComponent(in PixelEntity attacked, in PixelEntity attacker, int damage)
        {
            EntityId = attacked.Id;
            Attacker = attacker;
            Damage = damage;
        }
        public override int GetHashCode() => EntityId;
    }
}