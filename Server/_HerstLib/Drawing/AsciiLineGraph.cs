using System.Globalization;
using System.Text.RegularExpressions;

namespace HerstLib.Drawing
{
    public class AsciiLineGraph
    {
        public Dictionary<string, long> Data;
        public char[][] BufferArea { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int RealWidth { get; private set; }
        public int RealHeight { get; private set; }
        public Canvas Canvas { get; private set; }

        public AsciiLineGraph(int width, int height, Dictionary<string, long> data)
        {
            RealWidth = width;
            RealHeight = height;

            Width = width - 2;
            Height = height - 2;
            Data = data;
            BufferArea = new char[RealWidth][];

            for (var i = 0; i < RealWidth; i++)
            {
                BufferArea[i] = new char[RealHeight];
                for (var j = 0; j < RealHeight; j++)
                {
                    BufferArea[i][j] = ' ';
                }
            }
            Canvas = new Canvas(RealWidth, RealHeight);
        }

        // use DrawLine to create a line graph
        public string DrawGraph()
        {
            while (Data.Count > Width)
                Data = InterpolateData(Data);

            var spacingX = Width / Data.Count;
            var maxY = Data.Values.Max();
            var minY = Data.Values.Min();

            var lastX = 2;
            var lastY = (int)((Height / 2) + (-Normalize(Data.Values.First(), minY, maxY) * (Height / 2)));

            foreach (var item in Data)
            {
                var value = item.Value;
                var label = item.Key;

                var x = lastX + spacingX;
                var y = 1 + (int)((Height / 2) + (-Normalize(value, minY, maxY) * (Height / 2)));

                DrawLine(lastX, lastY, x, y);
                lastX = x;
                lastY = y;
            }
            DrawLegendHorizontal();
            DrawLegendVertical();

            return DrawBuffer();
        }

        private void DrawLegendHorizontal()
        {
            var first = Data.First().Key;
            var last = Data.Last().Key;

            DrawText(first, 1, RealHeight - 1);
            DrawText(last, RealWidth - 1 - last.Length, RealHeight - 1);
        }
        private void DrawLegendVertical()
        {
            var first = Data.First().Value;
            var last = Data.Last().Value;

            DrawTextVertical(first.ToString(), 1, RealHeight - 1);
            DrawTextVertical(last.ToString(), RealWidth - 1 - last.ToString().Length, RealHeight - 1);
        }
        private void DrawTextVertical(string text, int x, int y)
        {
            for (var i = 0; i < text.Length; i++)
            {
                BufferArea[x][y - i] = text[i];
            }
        }
        private void DrawText(string label, int x, int y)
        {
            for (var i = 0; i < label.Length; i++)
            {
                BufferArea[x + i][y] = label[i];
            }
        }

        private static Dictionary<string, long> InterpolateData(Dictionary<string, long> data)
        {
            var newData = new Dictionary<string, long>();
            var i = 0;

            foreach (var item in data)
            {
                if (i % 2 == 0)
                    newData.Add(item.Key, item.Value);
                i++;
            }

            return newData;
        }

        private static double Normalize(double val, double valmin, double valmax) => ((val - valmin) / (valmax - valmin) * (1 - -1)) + -1;

        // use DrawLine to draw a line between two points
        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            x1 = Math.Clamp(x1, 1, Width);
            x2 = Math.Clamp(x2, 1, Width);
            y1 = Math.Clamp(y1, 1, Height);
            y2 = Math.Clamp(y2, 1, Height);

            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var sx = x1 < x2 ? 1 : -1;
            var sy = y1 < y2 ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                Canvas.Set(x1, y1);

                if (x1 == x2 && y1 == y2)
                    break;

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        // use DrawBuffer to draw the buffer to the console
        public string DrawBuffer()
        {
            return Canvas.Frame();
            // var sb = new StringBuilder();

            // // draw axis lines
            // for (int i = 1; i < RealWidth; i++)
            //     BufferArea[i][RealHeight-2] = '-';

            // for (int i = 0; i < RealHeight-2; i++)
            //     BufferArea[1][i] = '|';

            // // rotate the buffer 180 degrees
            // for (int i = 1; i < RealHeight; i++)
            // {
            //     for (int j = 0; j < RealWidth; j++)
            //     {
            //         sb.Append(BufferArea[j][i]);
            //     }
            //     sb.Append(Environment.NewLine);
            // }

            // return sb.ToString();
        }
    }
    public partial class Canvas
    {
        private static readonly int[,] PIXEL_MAP = new int[,]
        {
            {0x1, 0x8},
            {0x2, 0x10},
            {0x4, 0x20},
            {0x40,0x80}
        };
        private readonly int[] chars;
        public int width;
        public int height;

        public Canvas(int width, int height)
        {
            if (width % 2 != 0)
            {
                width++;
            }
            if (height % 4 != 0)
            {
                height += 4 - (height % 4);
            }
            chars = new int[width * height / 8];
        }
        public void Clear()
        {
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = 0;
            }
        }
        public void Set(int x, int y)
        {
            var data = GetMethodData(x, y);
            SetInternal(data.Item1, data.Item2);
        }

        private void SetInternal(int coord, int mask) => chars[coord] |= mask;

        public void UnSet(int x, int y)
        {
            var data = GetMethodData(x, y);
            UnsetInternal(data.Item1, data.Item2);
        }
        private void UnsetInternal(int coord, int mask) => chars[coord] &= ~mask;

        public void Toggle(int x, int y)
        {
            var data = GetMethodData(x, y);
            ToggleInternal(data.Item1, data.Item2);
        }
        private void ToggleInternal(int coord, int mask) => chars[coord] ^= mask;

        private Tuple<int, int> GetMethodData(int x, int y)
        {
            var nx = x / 2;
            var ny = y / 4;
            var coord = nx + (width / 2 * ny);
            var mask = PIXEL_MAP[y % 4, x % 2];
            return new Tuple<int, int>(coord, mask);
        }

        public string Frame()
        {
            var result = string.Empty;
            for (int i = 0, j = 0; i < chars.Length; i++, j++)
            {
                if (j == width / 2)
                {
                    result += '\n';
                    j = 0;
                }
                if (chars[i] == 0)
                {
                    result += ' ';
                }
                else
                {
                    var newChar = "\\u" + (2800 + chars[i]);
                    result += UnescapeCodes(newChar);
                }
            }
            result += '\n';
            return result;
        }
        public static string UnescapeCodes(string src)
        {
            var rx = MyRegex();
            var result = src;
            result = rx.Replace(result, match => ((char)int.Parse(match.Value[2..], NumberStyles.HexNumber)).ToString());
            return result;
        }

        [GeneratedRegex("\\\\[uU]([0-9A-Fa-f]{4})")]
        private static partial Regex MyRegex();
    }
}