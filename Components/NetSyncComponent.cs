using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct NetSyncComponent
    {
        public readonly int EntityId;
        public readonly SyncThings Fields = SyncThings.None;

        public NetSyncComponent(int entityId, SyncThings fields)
        {
            EntityId = entityId;
            Fields = fields;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}