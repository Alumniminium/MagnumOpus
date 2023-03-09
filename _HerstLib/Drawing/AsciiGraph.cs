using System.Text;

namespace MagnumOpus._HerstLib.Drawing
{
    public class AsciiBarChart
    {
        public Dictionary<string, long> Data;

        public AsciiBarChart(Dictionary<string, long> data) => Data = data;
        public AsciiBarChart(List<long> data) => Data = data.ToDictionary(x => x.ToString(), x => x);

        public string[] DrawVertical(int height)
        {
            var sb = new StringBuilder();
            var values = Data.Values.ToArray();
            var min = Data.Values.Min();
            var max = Data.Values.Max();

            var lables = Data.Keys.ToArray();
            var maxLabelLength = lables.Max(x => x.Length);
            var maxBarheight = height - maxLabelLength - 1;

            for (var i = 0; i < values.Length; i++)
                values[i] = (long)Math.Round((double)values[i] / max * maxBarheight);

            for (var i = maxBarheight; i > 0; i--)
            {
                var legend = (long)(i != maxBarheight ? (float)max / maxBarheight * i : max);
                var strLegend = FormatNumber(legend) + "┫";

                if (i == 1)
                    _ = sb.Append(strLegend.PadLeft(5));
                else if (i % 4 == 0 && i != maxBarheight)
                    _ = sb.Append(strLegend.PadLeft(5));
                else if (i == maxBarheight)
                    _ = sb.Append(strLegend.PadLeft(5));
                else
                    _ = sb.Append("┫".PadLeft(5));

                for (long j = 0; j < Data.Keys.Count; j++)
                    _ = sb.Append(values[j] >= i ? " ███ " : "     ");
                _ = sb.AppendLine();
            }
            _ = sb.Append("┗".PadLeft(5));
            foreach (var label in lables)
                _ = sb.Append("━━╋━━");
            _ = sb.AppendLine();
            for (var i = 0; i < maxLabelLength; i++)
            {
                _ = sb.Append("     ");
                foreach (var label in lables)
                    _ = sb.Append(label.Length > i ? $"  {label[i]}  " : "    ");
                _ = sb.AppendLine();
            }

            return sb.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] DrawHorizontal(int width, bool compact = true)
        {
            var sb = new StringBuilder();
            var rawvalues = Data.Values.ToArray();
            var values = Data.Values.ToArray();
            var max = Data.Values.Max();

            var labels = Data.Keys.ToArray();
            var maxLabelLength = labels.Max(x => x.Length);
            var maxBarWidth = width - maxLabelLength - 1;

            for (long i = 0; i < values.Length; i++)
                values[i] = (long)Math.Round((double)values[i] / max * maxBarWidth);

            for (var i = 0; i < Data.Keys.Count; i++)
            {
                var legend = rawvalues[i];
                var strLegend = FormatNumber(legend);
                if (compact)
                    _ = sb.Append($"{$"{(labels[i] + ':').PadRight(maxLabelLength + 1)} {strLegend,4}".PadRight(1)}");
                else
                    _ = sb.Append($"{i.ToString().PadRight(Data.Keys.Count.ToString().Length)}");

                _ = sb.Append(' ');
                _ = sb.Append("┣".PadRight((int)values[i], '━'));
                _ = sb.AppendLine();
            }
            if (!compact)
            {
                _ = sb.AppendLine();
                for (var i = 0; i < labels.Length; i++)
                {
                    var legend = rawvalues[i];
                    var strLegend = FormatNumber(legend);
                    _ = sb.AppendLine($"{i.ToString().PadRight(2)} {(labels[i] + ':').PadRight(maxLabelLength + 1)} {rawvalues[i]}");
                }
            }

            return sb.ToString().Split(Environment.NewLine);
        }

        private static string FormatNumber(long legend)
        {
            var strLegend = legend.ToString();
            if (legend > 1000000000000)
                strLegend = $"{legend / 1000000000000}T";
            else if (legend > 1000000000)
                strLegend = $"{legend / 1000000000}G";
            else if (legend > 1000000)
                strLegend = $"{legend / 1000000}M";
            else if (legend > 1000)
                strLegend = $"{legend / 1000}K";
            return strLegend;
        }
    }
}