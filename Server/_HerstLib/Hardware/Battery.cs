namespace MagnumOpus._HerstLib.Hardware
{
    public class Battery
    {
        private static string path = "/sys/class/power_supply/BAT0/";

        public static float CapacityPercent { get; private set; }
        public static float ChargeFullMAh { get; private set; }
        public static float ChargeFullDesignMAh { get; private set; }
        public static float ChargeNowMAh { get; private set; }
        public static float VoltageV { get; private set; }
        public static float MinVoltageV { get; private set; }
        public static float CurrentMAh { get; private set; }
        public static int CycleCount { get; private set; }
        public static string? Status { get; private set; }
        public static string? Type { get; private set; }
        public static string? Model { get; private set; }
        public static string? Manufacturer { get; private set; }
        public static string? Serial { get; private set; }
        public static string? Technology { get; private set; }
        public static string? CapacityLevel { get; private set; }

        private static float GetCapacity() => float.Parse(File.ReadAllText(path + "capacity"));
        private static float GetChargeNow() => float.Parse(File.ReadAllText(path + "charge_now")) / 1000;
        private static float GetChargeFull() => float.Parse(File.ReadAllText(path + "charge_full")) / 1000;
        private static float GetChargeFullDesign() => float.Parse(File.ReadAllText(path + "charge_full_design")) / 1000;
        private static float GetVoltage() => float.Parse(File.ReadAllText(path + "voltage_now")) / 1000000;
        private static float GetMinVoltage() => float.Parse(File.ReadAllText(path + "voltage_min_design")) / 1000000;
        private static float GetCurrent() => float.Parse(File.ReadAllText(path + "current_now")) / 1000000;
        private static string GetStatus() => File.ReadAllText(path + "status").Trim();
        private static string GetBatteryType() => File.ReadAllText(path + "type").Trim();
        private static string GetModel() => File.ReadAllText(path + "model_name").Trim();
        private static string GetManufacturer() => File.ReadAllText(path + "manufacturer").Trim();
        private static string GetSerial() => File.ReadAllText(path + "serial_number").Trim();
        private static string GetTechnology() => File.ReadAllText(path + "technology").Trim();
        private static string GetCapacityLevel() => File.ReadAllText(path + "capacity_level").Trim();
        private static int GetCycleCount() => int.Parse(File.ReadAllText(path + "cycle_count"));

        public static void Scan(string batteryPath = "/sys/class/power_supply/BAT0/")
        {
            path = batteryPath;

            CapacityPercent = GetCapacity();
            ChargeFullMAh = GetChargeFull();
            ChargeFullDesignMAh = GetChargeFullDesign();
            VoltageV = GetVoltage();
            MinVoltageV = GetMinVoltage();
            CurrentMAh = GetCurrent();
            ChargeNowMAh = GetChargeNow();
            Status = GetStatus();
            Type = GetBatteryType();
            Model = GetModel();
            Manufacturer = GetManufacturer();
            Serial = GetSerial();
            Technology = GetTechnology();
            CapacityLevel = GetCapacityLevel();
            CycleCount = GetCycleCount();
        }
    }
}