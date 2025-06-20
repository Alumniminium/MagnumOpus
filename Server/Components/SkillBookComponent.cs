using System.Collections.Generic;
using ECS; // Assuming ECS namespace for [Component]

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct SkillBookComponent
    {
        public Dictionary<ushort, SkillData> Skills;

        public struct SkillData
        {
            public ushort Level;
            public ushort Experience;
        }

        // Parameterless constructor to initialize the Skills dictionary
        public SkillBookComponent()
        {
            Skills = new Dictionary<ushort, SkillData>();
        }
    }
}
