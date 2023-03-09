using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public readonly struct CqMonsterComponent
    {
        public readonly int CqMonsterId;

        [JsonConstructor]
        public CqMonsterComponent(int cq_monsterId) => CqMonsterId = cq_monsterId;
    }
}