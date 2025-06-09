using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Prometheus;

namespace MagnumOpus.IO;

public static partial class FConsole
{
    public static string StdoLogPath => $"{StartTime:dd-MM-yyyy}.log";
    private static readonly DateTime StartTime = DateTime.UtcNow;
    // private static StreamWriter Logger = new(StdoLogPath, true);
    private static readonly BlockingCollection<string> Lines = [];
    private static readonly Thread WorkerThread;

    private static readonly Counter MetricsExporter = Metrics.CreateCounter("MAGNUMOPUS_IO_FCONSOLE", "Debug Log Lines Written");

    static FConsole()
    {
        // Logger.AutoFlush = true;
        WorkerThread = new Thread(ProcessingQueue) { IsBackground = true };
        WorkerThread.Start();
    }

    private static void ProcessingQueue()
    {
        foreach (var line in Lines.GetConsumingEnumerable())
        {
            Console.Write(line);
            if (StartTime.Date != DateTime.UtcNow.Date)
                BeginNewFile();

            // Logger.Write($"[{DateTime.UtcNow}]{line}");
            MetricsExporter.Inc();
        }
    }

    private static void BeginNewFile()
    {
        // Logger.Close();
        // Logger.Dispose();
        // using var infs = new FileStream(StdoLogPath, FileMode.Open, FileAccess.Read);
        // using var outfs = new FileStream($"{StdoLogPath}.gz", FileMode.Create, FileAccess.Write);
        // using var gz = new GZipStream(outfs, CompressionMode.Compress);
        // infs.CopyTo(gz);
        // gz.Flush();
        // gz.Close();
        // infs.Close();
        // File.Delete(StdoLogPath);

        // StartTime = DateTime.UtcNow;
        // Logger = new(StdoLogPath, true)
        // {
        //     AutoFlush = true
        // };
        // GC.Collect();
    }

    public static void WriteLine(string line, params object[] objects)
    {
        try
        {
            // Handle both numbered placeholders {0}, {1} and named placeholders {caster}, {target}
            var numberedMatches = ArgMatcher().Matches(line);
            var namedMatches = NamedArgMatcher().Matches(line);

            if (numberedMatches.Count == 0 && namedMatches.Count == 0)
            {
                Lines.Add($"{line}{Environment.NewLine}");
                return;
            }

            if (numberedMatches.Count > 0)
            {
                // Handle numbered placeholders {0}, {1}, {2}, etc.
                var requiredCount = numberedMatches.Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).DefaultIfEmpty(-1).Max() + 1;
                if (objects.Length >= requiredCount)
                {
                    var formattedLine = string.Format(line, objects);
                    Lines.Add($"{formattedLine}{Environment.NewLine}");
                    return;
                }
            }
            else if (namedMatches.Count > 0)
            {
                // Handle named placeholders {caster}, {target}, etc.
                if (objects.Length >= namedMatches.Count)
                {
                    var formattedLine = line;
                    for (var i = 0; i < namedMatches.Count && i < objects.Length; i++)
                    {
                        var placeholder = namedMatches[i].Value;
                        formattedLine = formattedLine.Replace(placeholder, objects[i]?.ToString() ?? "null");
                    }
                    Lines.Add($"{formattedLine}{Environment.NewLine}");
                    return;
                }
            }

            Lines.Add($"{line}{Environment.NewLine}");
        }
        catch (FormatException ex)
        {
            Lines.Add($"[FormatException] {ex.Message}{Environment.NewLine}");
        }
    }

    public static void WriteSingleLine(string line) => Lines.Add($"{line.Replace(Environment.NewLine, " ")}{Environment.NewLine}");
    public static void Write(string text) => Lines.Add(text);
    public static void WriteLine(Exception e) => Lines.Add($"{e.Message}{Environment.NewLine}{e.StackTrace}{Environment.NewLine}{e.InnerException}{Environment.NewLine}");

    public static async ValueTask StopAsync()
    {
        Lines.CompleteAdding();
        while (Lines.Count > 0)
            await Task.Delay(100);
        // Logger.Close();
        WorkerThread.Join();
    }

    [GeneratedRegex(@"\{(\d+)\}")]
    private static partial Regex ArgMatcher();

    [GeneratedRegex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}")]
    private static partial Regex NamedArgMatcher();
}