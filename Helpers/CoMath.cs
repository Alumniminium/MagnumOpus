using System.Numerics;
using MagnumOpus.Enums;

namespace MagnumOpus.Helpers
{
    public static class CoMath
    {
        public static Direction GetDirection(Vector2 end, Vector2 start)
        {
            var angle = Math.Atan2(end.X - start.X, end.Y - start.Y) * 57.29 + 90;
            angle = angle < 0 ? 270 + (90 - Math.Abs(angle)) : angle;
            var direction = (byte)Math.Round(angle / 45.0);
            return (Direction)(direction == 8 ? 0 : direction);
        }

        public static float GetJumpTime(int distance)
        {
            var time = 0.0f;
            switch (distance)
            {
                case 0:
                    time = 0.8f;
                    break;
                case 1:
                    time = 0.8f;
                    break;
                case 2:
                    time = 0.8f;
                    break;
                case 3:
                    time = 0.85f;
                    break;
                case 4:
                    time = 0.85f;
                    break;
                case 5:
                    time = 0.9f;
                    break;
                case 6:
                    time = 0.9f;
                    break;
                case 7:
                    time = 1f;
                    break;
                case 8:
                    time = 1.1f;
                    break;
                case 9:
                    time = 1.1f;
                    break;
                case 10:
                    time = 1.1f;
                    break;
                case 11:
                    time = 1.2f;
                    break;
                case 12:
                    time = 1.2f;
                    break;
                case 13:
                    time = 1.35f;
                    break;
                case 14:
                    time = 1.4f;
                    break;
                case 15:
                    time = 1.45f;
                    break;
                case 16:
                    time = 1.45f;
                    break;
            }
            return time;
        }
    }
}