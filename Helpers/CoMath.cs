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

        public static bool IsInArc(Vector2 from, Vector2 end, Vector2 target, uint range)
        {
            const int defaultMagicArc = 90;
            const float radianDelta = (float)(Math.PI * defaultMagicArc / 180);

            var centerLine = GetRadian(from, end);
            var targetLine = GetRadian(from, target);
            var delta = Math.Abs(centerLine - targetLine);

            return (delta <= radianDelta || delta >= 2 * Math.PI - radianDelta) && GetLineLength(from, target) < range;
        }

        public static bool DdaLine(Vector2 start, Vector2 end, uint range, Vector2 target)
        {
            if (start == end)
                return false;

            // var scale = (float)(1.0f * nRange / Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)));
            var scale = (float)(1.0f * range / GetLineLength(start, end));
            var x0 = (int)start.X;
            var y0 = (int)start.Y;
            var x1 = (int)(0.5f + scale * (end.X - x0) + x0);
            var y1 = (int)(0.5f + scale * (end.Y - y0) + y0);
            return DdaLineEx(x0, y0, x1, y1, ref target);
        }

        /// <summary>
        /// Return all points on that line. (From TQ)
        /// </summary>
        private static bool DdaLineEx(int x0, int y0, int x1, int y1, ref Vector2 target)
        {
            if (x0 == x1 && y0 == y1)
                return false;
                
            var dx = x1 - x0;
            var dy = y1 - y0;
            var absDx = Math.Abs(dx);
            var absDy = Math.Abs(dy);
            Vector2 vector2;
            var delta = absDy * (dx > 0 ? 1 : -1);
            if (absDx > absDy)
            {
                delta = absDx * (dy > 0 ? 1 : -1);
                var numerator = dy * 2;
                var denominator = absDx * 2;
                if (dx > 0)
                {
                    // x0 ++
                    for (var i = 1; i <= absDx; i++)
                    {
                        vector2 = new Vector2((ushort)(x0 + i), (ushort)(y0 + (numerator * i + delta) / denominator));
                        if (vector2 == target)
                            return true;
                    }
                }
                else if (dx < 0)
                {
                    // x0 --
                    for (var i = 1; i <= absDx; i++)
                    {
                        vector2 = new Vector2((ushort)(x0 - i), (ushort)(y0 + (numerator * i + delta) / denominator));
                        if (vector2 == target)
                            return true;
                    }
                }
            }
            else
            {
                var numerator = dx * 2;
                var denominator = absDy * 2;
                if (dy > 0)
                {
                    // y0 ++
                    for (var i = 1; i <= absDy; i++)
                    {
                        vector2 = new Vector2((ushort)(y0 + i), (ushort)(x0 + (numerator * i + delta) / denominator));
                        if (vector2 == target)
                            return true;
                    }
                }
                else if (dy < 0)
                {
                    // y0 -- 
                    for (var i = 1; i <= absDy; i++)
                    {
                        vector2 = new Vector2((ushort)(y0 - i), (ushort)(x0 + (numerator * i + delta) / denominator));
                        if (vector2 == target)
                            return true;
                    }
                }
            }
            return false;
        }

        private static float GetRadian(Vector2 source, Vector2 target)
        {
            if (!(source.X != target.X || source.Y != target.Y))
                return 0f;

            var delta = target - source;
            var distance = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            if (!(delta.X <= distance && distance > 0))
                return 0f;
            var radian = Math.Asin(delta.X / distance);

            return (float)(delta.Y > 0 ? Math.PI / 2 - radian : Math.PI + radian + Math.PI / 2);
        }

        private static int GetLineLength(Vector2 from, Vector2 to)
        {
            var delta = to - from;
            return (int)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }
    }
}