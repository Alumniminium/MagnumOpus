namespace MagnumOpus.Components
{
    public readonly struct CqMonsterComponent
    {
        public readonly int EntityId;
        public readonly int CqMonsterId;

        public CqMonsterComponent(int entityId, int cq_monsterId)
        {
            EntityId = entityId;
            CqMonsterId = cq_monsterId;
        }

        public override int GetHashCode() => EntityId;
    }
}