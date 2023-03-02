using System.Numerics;

namespace MagnumOpus.Helpers
{
    public static class Constants
    {
        public static readonly Vector2[] DeltaPos = new Vector2[]
        {
            new Vector2(0, 1),
             new Vector2(-1, 1),
             new Vector2(-1, 0),
             new Vector2(-1, -1),
             new Vector2(0, -1),
             new Vector2(1, -1),
             new Vector2(1, 0),
            new Vector2(1, 1)
        };
    }
}