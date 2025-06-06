using System.Text;

namespace MagnumOpus._HerstLib.Hardware
{
    public class Cache
    {
        public string Path = "/sys/devices/system/cpu/cache/";
        public int Id => int.Parse(File.ReadAllText(Path + "id"));
        public string? Level => File.ReadAllText(Path + "level").Trim();
        public string? Type => File.ReadAllText(Path + "type").Trim();
        public int Size => int.Parse(File.ReadAllText(Path + "size").Replace("K", "")) * 1024;
        public int NumSets => int.Parse(File.ReadAllText(Path + "number_of_sets"));
        public int Associativity => int.Parse(File.ReadAllText(Path + "ways_of_associativity"));
        public int LineSize => int.Parse(File.ReadAllText(Path + "coherency_line_size"));
        public int PhysicalLinePartitions => int.Parse(File.ReadAllText(Path + "physical_line_partition"));
        public string SharedCoresList => File.ReadAllText(Path + "shared_cpu_list").Trim();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Id: {Id}");
            sb.AppendLine($"Level: {Level}");
            sb.AppendLine($"Type: {Type}");
            sb.AppendLine($"Size: {Size}");
            sb.AppendLine($"NumSets: {NumSets}");
            sb.AppendLine($"Associativity: {Associativity}");
            sb.AppendLine($"LineSize: {LineSize}");
            sb.AppendLine($"PhysicalLinePartitions: {PhysicalLinePartitions}");
            sb.AppendLine($"SharedCoresList: {string.Join(", ", SharedCoresList)}");

            return sb.ToString();
        }
    }
    public class CState
    {
        public string Path = "/sys/devices/system/cpu/cpuidle/";
        public string Name => File.ReadAllText(Path + "name").Trim();
        public string Description => File.ReadAllText(Path + "desc").Trim();
        public long Time => long.Parse(File.ReadAllText(Path + "time"));
        public long Usage => long.Parse(File.ReadAllText(Path + "usage"));
        public long Above => long.Parse(File.ReadAllText(Path + "above"));
        public long Below => long.Parse(File.ReadAllText(Path + "below"));
        public int Residency => int.Parse(File.ReadAllText(Path + "residency"));
        public int Latency => int.Parse(File.ReadAllText(Path + "latency"));
        public long Power => long.Parse(File.ReadAllText(Path + "power"));
        public bool Disabled => File.ReadAllText(Path + "disable").Trim() == "1";

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Description: {Description}");
            sb.AppendLine($"Time: {Time}");
            sb.AppendLine($"Usage: {Usage}");
            sb.AppendLine($"Above: {Above}");
            sb.AppendLine($"Below: {Below}");
            sb.AppendLine($"Residency: {Residency}");
            sb.AppendLine($"Latency: {Latency}");
            sb.AppendLine($"Power: {Power}");
            sb.AppendLine($"Disabled: {Disabled}");
            return sb.ToString();
        }
    }
    public class CPU
    {
        public string Path = "/sys/devices/system/cpu/";
        public int Frequency => int.Parse(File.ReadAllText(Path + "cpufreq/scaling_cur_freq"));
        public int MaxFrequency => int.Parse(File.ReadAllText(Path + "cpufreq/scaling_max_freq"));
        public int MinFrequency => int.Parse(File.ReadAllText(Path + "cpufreq/scaling_min_freq"));

        public string Governor => File.ReadAllText(Path + "cpufreq/scaling_governor").Trim();
        public string[] AvailableGovernors => File.ReadAllText(Path + "cpufreq/scaling_available_governors").Trim().Split(' ');
        public string[] AvailableFrequencies => File.ReadAllText(Path + "cpufreq/scaling_available_frequencies").Trim().Split(' ');

        public string ScalintDriver
        {
            get => File.ReadAllText(Path + "cpufreq/scaling_driver").Trim();
            set => File.WriteAllText(Path + "cpufreq/scaling_driver", value);
        }

        public bool CPB => File.ReadAllText(Path + "cpufreq/cpb").Trim() == "1";
        public int Bioslimit => int.Parse(File.ReadAllText(Path + "cpufreq/bios_limit"));

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Frequency: {Frequency} MHz");
            sb.AppendLine($"Max Frequency: {MaxFrequency} MHz");
            sb.AppendLine($"Min Frequency: {MinFrequency} MHz");
            sb.AppendLine($"Governor: {Governor}");
            sb.AppendLine($"Available Governors: {string.Join(", ", AvailableGovernors)}");
            sb.AppendLine($"Available Frequencies: {string.Join(", ", AvailableFrequencies)}");
            sb.AppendLine($"Scaling Driver: {ScalintDriver}");
            sb.AppendLine($"CPB: {CPB}");
            sb.AppendLine($"Bioslimit: {Bioslimit}");
            return sb.ToString();
        }
    }
    public static class CPUs
    {
        public const string PathRoot = "/sys/devices/system/cpu/";

        public static int Count => int.Parse(File.ReadAllText(PathRoot + "present").Split('-')[1]) + 1;
        public static CPU[] GetCores()
        {
            var cores = new CPU[Count];
            for (var i = 0; i < Count; i++)
            {
                cores[i] = new CPU
                {
                    Path = PathRoot + $"cpu{i}/"
                };
            }
            return cores;
        }

        public static CState[] GetCStates()
        {
            var cpus = GetCores();
            var cstates = new List<CState>();
            for (var i = 0; i < cpus.Length; i++)
            {
                var path = $"{PathRoot}cpu{i}/cpuidle/";
                var count = Directory.GetFileSystemEntries(path).Length;

                var states = new CState[count];

                for (var x = 0; x < count; x++)
                {
                    states[x] = new CState
                    {
                        Path = path + "state" + x + "/"
                    };
                }
                cstates.AddRange(states);
            }
            return cstates.ToArray();
        }

        public static Cache[] GetCaches()
        {
            var cpus = GetCores();
            var caches = new List<Cache>();
            for (var i = 0; i < cpus.Length; i++)
            {
                var path = $"{PathRoot}cpu{i}/cache/";
                var count = Directory.GetFileSystemEntries(path).Length - 1;

                var states = new Cache[count];

                for (var x = 0; x < count; x++)
                {
                    states[x] = new Cache
                    {
                        Path = path + "index" + x + "/"
                    };
                }
                caches.AddRange(states);
            }
            return caches.ToArray();
        }

        public static bool SMT
        {
            get => File.ReadAllText(PathRoot + "smt/active").Trim() == "1";
            set => File.WriteAllText(PathRoot + "smt/control", value ? "1" : "0");
        }

    }
}