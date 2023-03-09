using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Leveling
{
    [Component]
    [Save]
    public struct ExpRewardComponent
    {
        public int Experience;

        [JsonConstructor]
        public ExpRewardComponent(int experience) => Experience = experience;
    }
}