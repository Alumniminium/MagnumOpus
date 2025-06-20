using System.Collections.Generic;
using NttECS.ECS; // Assuming ECS namespace for [Component]

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct FriendListComponent
    {
        public HashSet<int> FriendIds;

        // Parameterless constructor to initialize the FriendIds HashSet
        public FriendListComponent()
        {
            FriendIds = new HashSet<int>();
        }
    }
}
