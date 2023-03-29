using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;

namespace HerstLib.IO
{
    public static class FConsole
    {
        public static string StdoLogPath => $"{StartTime:dd-MM-yyyy}.log";
        private static readonly DateTime StartTime = DateTime.UtcNow;
        // private static StreamWriter Logger = new(StdoLogPath, true);
        private static readonly BlockingCollection<string> Lines = new();
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

        public static void WriteLine(string line) => Lines.Add($"{line}{Environment.NewLine}");
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
    }
}