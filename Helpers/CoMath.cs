using System.Numerics;
using MagnumOpus.Enums;

namespace MagnumOpus.Helpers
{
    public static class CoMath
    {
        public static double GetDistance(Vector2 start, Vector2 end)
        {
            return Math.Sqrt((start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y));
        }
        public static Direction GetDirection(Vector2 end, Vector2 start) => (Direction)(GetRawDirection(end, start) % 8);
        public static byte GetRawDirection(Vector2 end, Vector2 start)
        {
            var dir = 0;

            var tan = new[] { -241, -41, 41, 241 };
            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            if (deltaX == 0)
            {
                dir = deltaY > 0 ? 0 : 4;
            }
            else if (deltaY == 0)
            {
                dir = deltaX > 0 ? 6 : 2;
            }
            else
            {
                var flag = Math.Abs(deltaX) / deltaX;

                deltaY *= 100 * flag;
                int i;
                for (i = 0; i < 4; i++)
                    tan[i] *= (int)Math.Abs(deltaX);

                for (i = 0; i < 3; i++)
                    if (deltaY >= tan[i] && deltaY < tan[i + 1])
                        break;

                //** note :
                //   i=0    ------- -241 -- -41
                //   i=1    ------- -41  --  41
                //   i=2    -------  41  -- 241
                //   i=3    -------  241 -- -241

                deltaX = end.X - start.X;
                deltaY = end.Y - start.Y;

                if (deltaX > 0)
                {
                    switch (i)
                    {
                        case 0:
                            dir = 5;
                            break;
                        case 1:
                            dir = 6;
                            break;
                        case 2:
                            dir = 7;
                            break;
                        case 3:
                            dir = deltaY > 0 ? 0 : 4;
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            dir = 1;
                            break;
                        case 1:
                            dir = 2;
                            break;
                        case 2:
                            dir = 3;
                            break;
                        case 3:
                            dir = deltaY > 0 ? 0 : 4;
                            break;
                    }
                }
            }

            dir += 8;
            return (byte)dir;
        }

        public static float GetJumpTime(int distance)
        {
            var time = 0.0f;
            switch (distance)
            {
                case 0:
                    time = 0.7f;
                    break;
                case 1:
                    time = 0.7f;
                    break;
                case 2:
                    time = 0.75f;
                    break;
                case 3:
                    time = 0.75f;
                    break;
                case 4:
                    time = 0.8f;
                    break;
                case 5:
                    time = 0.8f;
                    break;
                case 6:
                    time = 0.9f;
                    break;
                case 7:
                    time = 0.95f;
                    break;
                case 8:
                    time = 0.95f;
                    break;
                case 9:
                    time = 1.00f;
                    break;
                case 10:
                    time = 1.05f;
                    break;
                case 11:
                    time = 1.10f;
                    break;
                case 12:
                    time = 1.15f;
                    break;
                case 13:
                    time = 1.20f;
                    break;
                case 14:
                    time = 1.25f;
                    break;
                case 15:
                    time = 1.3f;
                    break;
                case 16:
                    time = 1.3f;
                    break;
            }
            return time;
        }
    }
}