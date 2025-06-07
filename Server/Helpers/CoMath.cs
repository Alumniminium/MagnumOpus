using System.Drawing;
using System.Numerics;
using MagnumOpus.Enums;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Mathematical utility functions for game mechanics including geometry, pathfinding, and spatial calculations.
    /// Provides Conquer Online specific algorithms for movement, targeting, and area-of-effect calculations.
    /// </summary>
    public static class CoMath
    {
        /// <summary>
        /// Generates a random point within the specified rectangle bounds.
        /// </summary>
        /// <param name="rect">Rectangle defining the boundary area</param>
        /// <returns>Random point within the rectangle</returns>
        public static Vector2 GetRandomPointInRect(in Rectangle rect)
        {
            var randX = Random.Shared.Next(rect.X, rect.X + rect.Width + 1);
            var randY = Random.Shared.Next(rect.Y, rect.Y + rect.Height + 1);
            return new Vector2(randX, randY);
        }

        /// <summary>
        /// Determines if two points are within the specified range using squared distance calculation for performance.
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">Target position</param>
        /// <param name="range">Maximum distance threshold</param>
        /// <returns>True if points are within range</returns>
        public static bool InRange(Vector2 start, Vector2 end, double range)
        {
            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;
            var distance = (deltaX * deltaX) + (deltaY * deltaY);
            return distance <= range * range;
        }
        /// <summary>
        /// Gets the 8-directional movement direction from start to end position.
        /// </summary>
        /// <param name="end">Target position</param>
        /// <param name="start">Starting position</param>
        /// <returns>8-directional enum value</returns>
        public static Direction GetDirection(Vector2 end, Vector2 start) => (Direction)(GetRawDirection(end, start) % 8);
        
        /// <summary>
        /// Converts a directional vector to the nearest 8-directional enum using angle calculation.
        /// </summary>
        /// <param name="direction">Direction vector to convert</param>
        /// <returns>Nearest 8-directional enum value</returns>
        public static Direction GetNearestDirection(Vector2 direction)
        {
            var angle = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI));
            angle = (angle + 360) % 360; // Normalize angle to be in the range of [0, 360)

            if (angle >= 337.5 || angle < 22.5)
                return Direction.East;
            if (angle >= 22.5 && angle < 67.5)
                return Direction.NorthEast;
            if (angle >= 67.5 && angle < 112.5)
                return Direction.North;
            if (angle >= 112.5 && angle < 157.5)
                return Direction.NorthWest;
            if (angle >= 157.5 && angle < 202.5)
                return Direction.West;
            if (angle >= 202.5 && angle < 247.5)
                return Direction.SouthWest;
            if (angle >= 247.5 && angle < 292.5)
                return Direction.South;
            return Direction.SouthEast; // 292.5 <= angle < 337.5
        }

        /// <summary>
        /// Calculates raw directional value using Conquer Online's tangent-based direction algorithm.
        /// Uses slope calculations with predefined tangent thresholds for 8-directional movement.
        /// </summary>
        /// <param name="end">Target position</param>
        /// <param name="start">Starting position</param>
        /// <returns>Raw direction value (0-15 range)</returns>
        public static byte GetRawDirection(Vector2 end, Vector2 start)
        {
            var dir = 0;

            var tan = new[] { -241, -41, 41, 241 };
            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            if (deltaX == 0)
                dir = deltaY > 0 ? 0 : 4;
            else if (deltaY == 0)
                dir = deltaX > 0 ? 6 : 2;
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
                        default:
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
                        default:
                            break;
                    }
                }
            }

            dir += 8;
            return (byte)dir;
        }

        /// <summary>
        /// Calculates jump animation time based on distance using Conquer Online's jump timing formula.
        /// Provides realistic jump duration scaling from 0.7s to 1.3s based on distance.
        /// </summary>
        /// <param name="distance">Jump distance in tiles</param>
        /// <returns>Jump animation time in seconds</returns>
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
                default:
                    break;
            }
            return time;
        }

        /// <summary>
        /// Determines if a target position is within a sector (cone) area-of-effect from origin to click point.
        /// </summary>
        /// <param name="pos">Origin position of the sector</param>
        /// <param name="click">Direction point defining sector center</param>
        /// <param name="check">Position to test for inclusion in sector</param>
        /// <param name="widthRadians">Sector width in radians</param>
        /// <returns>True if target position is within the sector</returns>
        public static bool InSector(Vector2 pos, Vector2 click, Vector2 check, float widthRadians)
        {
            double aimRad = GetRadian(pos, click);
            double checkRad = GetRadian(pos, check);

            if (aimRad - (widthRadians / 2) < checkRad)
                if (aimRad + (widthRadians / 2) > checkRad)
                    return true;
            return false;
        }

        /// <summary>
        /// Tests if a target point lies on a line from start to end within specified range using DDA algorithm.
        /// Uses Digital Differential Analyzer for precise line-based targeting calculations.
        /// </summary>
        /// <param name="start">Line starting position</param>
        /// <param name="end">Line ending position</param>
        /// <param name="range">Maximum line length</param>
        /// <param name="target">Position to test for line intersection</param>
        /// <returns>True if target lies on the line path</returns>
        public static bool DdaLine(Vector2 start, Vector2 end, uint range, Vector2 target)
        {
            if (start == end)
                return false;

            var scale = (float)(1.0f * range / Vector2.Distance(start, end));
            var x0 = (int)start.X;
            var y0 = (int)start.Y;
            var x1 = (int)(0.5f + (scale * (end.X - x0)) + x0);
            var y1 = (int)(0.5f + (scale * (end.Y - y0)) + y0);
            return DdaLineEx(x0, y0, x1, y1, ref target);
        }

        /// <summary>
        /// Digital Differential Analyzer implementation for line rasterization and target detection.
        /// Returns all points on the line path from TQ's original algorithm.
        /// </summary>
        /// <param name="x0">Starting X coordinate</param>
        /// <param name="y0">Starting Y coordinate</param>
        /// <param name="x1">Ending X coordinate</param>
        /// <param name="y1">Ending Y coordinate</param>
        /// <param name="target">Target position to find on line</param>
        /// <returns>True if target position is found on the line</returns>
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
                        vector2 = new Vector2((ushort)(x0 + i), (ushort)(y0 + (((numerator * i) + delta) / denominator)));
                        if (vector2 == target)
                            return true;
                    }
                }
                else if (dx < 0)
                {
                    // x0 --
                    for (var i = 1; i <= absDx; i++)
                    {
                        vector2 = new Vector2((ushort)(x0 - i), (ushort)(y0 + (((numerator * i) + delta) / denominator)));
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
                        vector2 = new Vector2((ushort)(y0 + i), (ushort)(x0 + (((numerator * i) + delta) / denominator)));
                        if (vector2 == target)
                            return true;
                    }
                }
                else if (dy < 0)
                {
                    // y0 -- 
                    for (var i = 1; i <= absDy; i++)
                    {
                        vector2 = new Vector2((ushort)(y0 - i), (ushort)(x0 + (((numerator * i) + delta) / denominator)));
                        if (vector2 == target)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Calculates angle in radians between two positions for directional calculations.
        /// </summary>
        /// <param name="source">Starting position</param>
        /// <param name="target">Target position</param>
        /// <returns>Angle in radians from source to target</returns>
        private static float GetRadian(Vector2 source, Vector2 target)
        {
            if (!(source.X != target.X || source.Y != target.Y))
                return 0f;

            var delta = target - source;
            var distance = (float)Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));

            if (!(delta.X <= distance && distance > 0))
                return 0f;
            var radian = Math.Asin(delta.X / distance);

            return (float)(delta.Y > 0 ? (Math.PI / 2) - radian : Math.PI + radian + (Math.PI / 2));
        }

        /// <summary>
        /// Determines if two positions are within screen distance (18 tiles in both X and Y directions).
        /// Based on Conquer Online's official screen system specification.
        /// </summary>
        /// <param name="x1">X coordinate of first position</param>
        /// <param name="y1">Y coordinate of first position</param>
        /// <param name="x2">X coordinate of second position</param>
        /// <param name="y2">Y coordinate of second position</param>
        /// <returns>True if positions are within screen distance</returns>
        public static bool InScreen(float x1, float y1, float x2, float y2)
        {
            const int SCREEN_DISTANCE = 18;
            return Math.Abs(x1 - x2) <= SCREEN_DISTANCE && Math.Abs(y1 - y2) <= SCREEN_DISTANCE;
        }

        /// <summary>
        /// Determines if two positions are within screen distance (18 tiles in both X and Y directions).
        /// </summary>
        public static bool InScreen(Vector2 pos1, Vector2 pos2)
        {
            return InScreen(pos1.X, pos1.Y, pos2.X, pos2.Y);
        }

        /// <summary>
        /// Converts a direction enum to a unit vector for movement calculations.
        /// </summary>
        /// <param name="direction">8-directional enum value</param>
        /// <returns>Unit vector representing the direction</returns>
        /// <exception cref="ArgumentException">Thrown for invalid direction values</exception>
        internal static Vector2 DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.North => new Vector2(0, 1),
                Direction.NorthEast => new Vector2(1, 1),
                Direction.East => new Vector2(1, 0),
                Direction.SouthEast => new Vector2(1, -1),
                Direction.South => new Vector2(0, -1),
                Direction.SouthWest => new Vector2(-1, -1),
                Direction.West => new Vector2(-1, 0),
                Direction.NorthWest => new Vector2(-1, 1),
                _ => throw new ArgumentException($"Invalid direction: {direction}"),
            };
        }
    }
}